<#
  Deletes GitHub Actions WORKFLOW RUNS (with their logs and artifacts) whose branch has been
  MERGED (via a PR, into any branch) or NO LONGER EXISTS, once they are more than 7 days old
  (-MinAgeDays). Runs on main and gh-pages (docs deployments) are deleted once they are more than
  30 days old (-MainMinAgeDays).

  Always kept:
    - Runs on main and gh-pages created within the last 30 days
    - Runs for tags (release tags and any other tag)
    - Runs on any branch that has an OPEN (or draft) pull request
    - Runs that are not completed (queued / in progress)
    - Runs created within the last 7 days
    - Runs triggered from forks, and runs with no branch

  Also deletes Actions CACHES that can never be used again: caches created by a pull request
  that is no longer open, and caches for branches that no longer exist. (A PR's caches can only
  be read by that PR; GitHub evicts everything else itself - unused for 7 days, or least
  recently used once the repo is over its cache limit.)

  Works everything out live from the GitHub API each time - no ID list needed.
  Run by .github/workflows/cleanup-workflow-runs.yml; can also be run locally.

  Requires: GitHub CLI (gh), signed in (or GH_TOKEN set) with actions:write on the repo.

  Usage:
    ./Remove-StaleWorkflowRuns.ps1            # dry run - writes the plan to a CSV, deletes nothing
    ./Remove-StaleWorkflowRuns.ps1 -Delete    # actually deletes (permanent - logs, artifacts and caches)
    ./Remove-StaleWorkflowRuns.ps1 -MinAgeDays 14 -MainMinAgeDays 60
#>
param(
    [switch]$Delete,
    [string]$Repo = $env:GITHUB_REPOSITORY,             # owner/name; defaults to the repo the workflow runs in
    [ValidateRange(1, 3650)][int]$MinAgeDays = 7,       # merged/deleted-branch runs older than this are deleted
    [ValidateRange(1, 3650)][int]$MainMinAgeDays = 30,  # main/gh-pages runs older than this are deleted
    [ValidateRange(8, 3650)][int]$LookbackDays = 500,   # how far back to look (GitHub keeps runs up to ~400 days)
    [ValidateRange(1, 31)][int]$WindowDays = 10,        # size of each date window when listing runs
    [string]$OutDir = $(if ($env:RUNNER_TEMP) { $env:RUNNER_TEMP } else { (Get-Location).Path })
)

$ErrorActionPreference = 'Continue'   # gh writes progress to stderr; handle exit codes manually
if (-not $Repo) { $Repo = 'cabinetoffice/GCGS-Central-Digital-Platform' }
$LongLivedBranches = @('main', 'gh-pages')   # never merged or deleted, so aged out on -MainMinAgeDays
$Stamp   = '{0:yyyyMMdd_HHmmss}' -f (Get-Date)
$LogFile = Join-Path $OutDir "stale_workflow_runs_$Stamp.log"
$PlanCsv = Join-Path $OutDir "stale_workflow_runs_plan_$Stamp.csv"
$CachePlanCsv = Join-Path $OutDir "stale_workflow_runs_cache_plan_$Stamp.csv"

function Invoke-GhLines([string[]]$GhArgs, [int]$Retries = 3) {
    # Runs gh, retrying transient failures (GitHub sometimes returns 502/timeouts on large listings).
    # stderr goes to a temp file so the real error message can be shown if it keeps failing.
    $errFile = [System.IO.Path]::GetTempFileName()
    try {
        for ($attempt = 1; $attempt -le $Retries; $attempt++) {
            $out = & gh @GhArgs 2> $errFile
            if ($LASTEXITCODE -eq 0) { return @($out | Where-Object { $_ -ne $null -and $_ -ne '' }) }
            $msg = "$(Get-Content $errFile -Raw)".Trim()
            if (-not $msg) { $msg = 'no error text from gh' }
            if ($attempt -lt $Retries) {
                Write-Host "  gh call failed (attempt $attempt of $Retries), retrying in $(5 * $attempt)s: $msg"
                Start-Sleep -Seconds (5 * $attempt)
            }
        }
        throw "gh $($GhArgs -join ' ') failed (exit $LASTEXITCODE): $msg"
    } finally { Remove-Item $errFile -ErrorAction SilentlyContinue }
}

function Invoke-GhDelete([string]$Path) {
    # DELETEs an API path, retrying up to 3 times and waiting out the rate limit if it is hit.
    for ($attempt = 1; $attempt -le 3; $attempt++) {
        gh api -X DELETE $Path *> $null
        if ($LASTEXITCODE -eq 0) { return $true }
        $reset = gh api rate_limit --jq '.resources.core.reset' 2>$null
        $remaining = gh api rate_limit --jq '.resources.core.remaining' 2>$null
        if ($remaining -and [int]$remaining -lt 5 -and $reset) {
            $wait = [math]::Max(5, [int64]$reset - [DateTimeOffset]::UtcNow.ToUnixTimeSeconds() + 5)
            Write-Host "Rate limit reached - waiting $wait seconds..."
            Start-Sleep -Seconds $wait
        } else { Start-Sleep -Seconds 2 }
    }
    return $false
}

function Write-Summary([string]$Text) {
    # Appends to the job summary when running in GitHub Actions; no-op locally.
    if ($env:GITHUB_STEP_SUMMARY) { Add-Content -Path $env:GITHUB_STEP_SUMMARY -Value $Text }
}

if (-not (Get-Command gh -ErrorAction SilentlyContinue)) { throw 'gh CLI not found on PATH.' }
gh auth status *> $null
if ($LASTEXITCODE -ne 0) { throw 'gh is not signed in. Run: gh auth login (or set GH_TOKEN)' }

$Cutoff     = [DateTimeOffset]::UtcNow.AddDays(-$MinAgeDays)
$MainCutoff = [DateTimeOffset]::UtcNow.AddDays(-$MainMinAgeDays)
$Mode       = if ($Delete) { 'DELETE' } else { 'DRY RUN' }
Write-Host "Repo: $Repo  |  Mode: $Mode  |  Only runs created before $($Cutoff.ToString('yyyy-MM-dd HH:mm')) UTC ($MinAgeDays+ days old)"
Write-Host "  main/gh-pages: only runs created before $($MainCutoff.ToString('yyyy-MM-dd HH:mm')) UTC ($MainMinAgeDays+ days old)"

# 1. Existing branches
Write-Host 'Reading branches...'
$branches = [System.Collections.Generic.HashSet[string]]::new()
Invoke-GhLines @('api', '--paginate', "repos/$Repo/branches?per_page=100", '--jq', '.[].name') | ForEach-Object { [void]$branches.Add($_) }
if (-not $branches.Contains('main')) { throw "Branch list looks wrong (main not found) - aborting." }

# 2. Tags (runs for any tag are kept)
Write-Host 'Reading tags...'
$tags = [System.Collections.Generic.HashSet[string]]::new()
Invoke-GhLines @('api', '--paginate', "repos/$Repo/tags?per_page=100", '--jq', '.[].name') | ForEach-Object { [void]$tags.Add($_) }

# 3. Pull requests: which head branches are merged, which have an open PR
Write-Host 'Reading pull requests...'
$merged   = [System.Collections.Generic.HashSet[string]]::new()
$openHead = [System.Collections.Generic.HashSet[string]]::new()
$openPrs  = [System.Collections.Generic.HashSet[string]]::new()   # PR numbers, including forks (for caches)
$prLines  = Invoke-GhLines @('api', '--paginate', "repos/$Repo/pulls?state=all&per_page=100", '--jq',
            '.[] | [.state, (.merged_at != null), .head.ref, .head.repo.full_name, .number] | @tsv')
foreach ($l in $prLines) {
    $state, $isMerged, $head, $headRepo, $number = $l -split "`t", 5
    if ($state -eq 'open') { [void]$openPrs.Add($number) }
    if ($headRepo -ne $Repo) { continue }                  # ignore fork PRs
    if ($state -eq 'open') { [void]$openHead.Add($head) }
    elseif ($isMerged -eq 'true') { [void]$merged.Add($head) }
}
Write-Host ("  branches: {0}  tags: {1}  merged PR branches: {2}  open PR branches: {3}" -f $branches.Count, $tags.Count, $merged.Count, $openHead.Count)

# 4. Workflow runs - fetched in small date windows. A single listing of every run in a large
#    repo can time out, and GitHub caps any filtered listing at 1,000 results, so each window
#    must stay under that. Only runs older than the cutoff are needed.
Write-Host "Reading workflow runs older than $MinAgeDays days (last $LookbackDays days, in $WindowDays-day windows)..."
$runLines = [System.Collections.Generic.List[string]]::new()
$windowEnd = $Cutoff.UtcDateTime.Date                       # up to (not including) the cutoff day
$start     = [DateTime]::UtcNow.Date.AddDays(-$LookbackDays)
$jqRuns    = '.workflow_runs[] | [.id, .head_branch, .event, .status, .created_at, .name, .head_repository.full_name] | @tsv'
for ($from = $start; $from -lt $windowEnd; $from = $from.AddDays($WindowDays)) {
    $to = $from.AddDays($WindowDays - 1)
    if ($to -ge $windowEnd) { $to = $windowEnd.AddDays(-1) }
    $range = '{0:yyyy-MM-dd}..{1:yyyy-MM-dd}' -f $from, $to
    $lines = Invoke-GhLines @('api', '--paginate', "repos/$Repo/actions/runs?per_page=100&created=$range", '--jq', $jqRuns)
    if ($lines.Count -ge 1000) { Write-Host "  WARNING: $range returned 1,000 runs (GitHub's cap) - some may be missed; re-run with -WindowDays 1" }
    foreach ($l in $lines) { $runLines.Add($l) }
    if ($lines.Count) { Write-Host ("  {0}: {1} runs" -f $range, $lines.Count) }
}
# Include runs from the cutoff day itself; the per-run age check below filters exact times.
$lines = Invoke-GhLines @('api', '--paginate', ("repos/$Repo/actions/runs?per_page=100&created={0:yyyy-MM-dd}" -f $windowEnd), '--jq', $jqRuns)
foreach ($l in $lines) { $runLines.Add($l) }
Write-Host "  runs found: $($runLines.Count)"

# 5. Decide
$plan = [System.Collections.Generic.List[object]]::new()
$kept = [ordered]@{ main = 0; tag = 0; openPr = 0; activeBranch = 0; notCompleted = 0; fork = 0; noBranch = 0; tooNew = 0 }
foreach ($l in $runLines) {
    $id, $branch, $event, $status, $created, $wf, $headRepo = $l -split "`t", 7
    if (-not $branch)                       { $kept.noBranch++;     continue }
    if ($headRepo -and $headRepo -ne $Repo) { $kept.fork++;         continue }
    if ($status -ne 'completed')            { $kept.notCompleted++; continue }
    $createdAt = [DateTimeOffset]::MinValue
    if (-not [DateTimeOffset]::TryParse($created, [ref]$createdAt)) { $kept.tooNew++; continue }   # unreadable date - keep
    if ($createdAt -ge $Cutoff)             { $kept.tooNew++;       continue }

    $reason = $null
    if ($LongLivedBranches -contains $branch) {
        if ($createdAt -ge $MainCutoff)     { $kept.main++;         continue }
        $reason = "$branch, $MainMinAgeDays+ days old"
    } else {
        if ($tags.Contains($branch))        { $kept.tag++;          continue }
        if ($openHead.Contains($branch))    { $kept.openPr++;       continue }
        if ($merged.Contains($branch))      { $reason = 'branch merged' }
        elseif (-not $branches.Contains($branch)) { $reason = 'branch deleted' }
        if (-not $reason)                   { $kept.activeBranch++; continue }
    }

    $plan.Add([pscustomobject]@{ RunId = $id; Workflow = $wf; Branch = $branch; Event = $event; Created = $created; Reason = $reason })
}

$plan | Export-Csv -Path $PlanCsv -NoTypeInformation -Encoding UTF8
$byReason = @($plan | Group-Object Reason | Sort-Object Name)
Write-Host ("Runs to delete: {0}  ({1})" -f $plan.Count, (($byReason | ForEach-Object { "$($_.Name): $($_.Count)" }) -join ', '))
Write-Host ("Kept - main/gh-pages (newer than {0} days): {1}, tags: {2}, open-PR branches: {3}, other live branches: {4}, not completed: {5}, forks: {6}, no branch: {7}, newer than {8} days: {9}" -f `
    $MainMinAgeDays, $kept.main, $kept.tag, $kept.openPr, $kept.activeBranch, $kept.notCompleted, $kept.fork, $kept.noBranch, $MinAgeDays, $kept.tooNew)
Write-Host "Plan written to: $PlanCsv"

# 6. Caches - only ones nothing can read again: a PR's caches are scoped to that PR, and a
#    branch's caches to that branch, so once the PR is closed or the branch is gone they are dead
#    weight that pushes the repo over its cache limit and evicts caches main still uses.
Write-Host 'Reading caches...'
$cachePlan = [System.Collections.Generic.List[object]]::new()
$cacheKept = 0; $cacheKeptBytes = [int64]0
$maxPr = ($prLines | ForEach-Object { [int](($_ -split "`t")[4]) } | Measure-Object -Maximum).Maximum
$cacheLines = Invoke-GhLines @('api', '--paginate', "repos/$Repo/actions/caches?per_page=100", '--jq',
              '.actions_caches[] | [.id, .ref, .key, .size_in_bytes, .last_accessed_at] | @tsv')
foreach ($l in $cacheLines) {
    $cacheId, $ref, $key, $size, $lastUsed = $l -split "`t", 5
    $reason = $null
    if ($ref -match '^refs/pull/(\d+)/') {
        # PRs opened after the PR list was read are newer than $maxPr - keep their caches
        if (-not $openPrs.Contains($Matches[1]) -and [int]$Matches[1] -le $maxPr) { $reason = 'PR closed' }
    } elseif ($ref -match '^refs/heads/(.+)$') {
        if (-not $branches.Contains($Matches[1])) { $reason = 'branch deleted' }
    }
    if (-not $reason) { $cacheKept++; $cacheKeptBytes += [int64]$size; continue }
    $cachePlan.Add([pscustomobject]@{ CacheId = $cacheId; Ref = $ref; Key = $key; SizeBytes = [int64]$size; LastUsed = $lastUsed; Reason = $reason })
}
$cachePlan | Export-Csv -Path $CachePlanCsv -NoTypeInformation -Encoding UTF8
$cacheBytes = [int64]($cachePlan | Measure-Object SizeBytes -Sum).Sum
$cacheByReason = @($cachePlan | Group-Object Reason | Sort-Object Name)
Write-Host ("Caches to delete: {0} ({1:N0} MB)  ({2})" -f $cachePlan.Count, ($cacheBytes / 1MB), (($cacheByReason | ForEach-Object { "$($_.Name): $($_.Count)" }) -join ', '))
Write-Host ("Caches kept: {0} ({1:N0} MB)" -f $cacheKept, ($cacheKeptBytes / 1MB))
Write-Host "Cache plan written to: $CachePlanCsv"

Write-Summary "## Workflow run cleanup ($Mode)"
Write-Summary ''
Write-Summary "Runs found older than $MinAgeDays days: **$($runLines.Count)**. Planned for deletion: **$($plan.Count)**."
Write-Summary ''
Write-Summary '| To delete | Runs |'
Write-Summary '|---|---|'
foreach ($g in $byReason) { Write-Summary "| $($g.Name) | $($g.Count) |" }
Write-Summary ''
Write-Summary '| Kept | Runs |'
Write-Summary '|---|---|'
Write-Summary "| main/gh-pages, newer than $MainMinAgeDays days | $($kept.main) |"
Write-Summary "| Tags | $($kept.tag) |"
Write-Summary "| Branches with an open PR | $($kept.openPr) |"
Write-Summary "| Other live branches | $($kept.activeBranch) |"
Write-Summary "| Not completed | $($kept.notCompleted) |"
Write-Summary "| Forks | $($kept.fork) |"
Write-Summary "| No branch | $($kept.noBranch) |"
Write-Summary "| Newer than $MinAgeDays days | $($kept.tooNew) |"
Write-Summary ''
Write-Summary ("Caches planned for deletion: **{0}** ({1:N0} MB). Kept: {2} ({3:N0} MB)." -f $cachePlan.Count, ($cacheBytes / 1MB), $cacheKept, ($cacheKeptBytes / 1MB))
Write-Summary ''
Write-Summary '| Caches to delete | Caches | MB |'
Write-Summary '|---|---|---|'
foreach ($g in $cacheByReason) { Write-Summary ("| {0} | {1} | {2:N0} |" -f $g.Name, $g.Count, (($g.Group | Measure-Object SizeBytes -Sum).Sum / 1MB)) }

if (-not $Delete) {
    Write-Host 'Dry run - nothing deleted. Re-run with -Delete to delete these runs and caches.'
    Write-Summary ''
    Write-Summary 'Dry run - nothing was deleted.'
    return
}

# 7. Delete runs
$deleted = 0; $failed = 0
foreach ($p in $plan) {
    $createdAt = [DateTimeOffset]::Parse($p.Created)
    if ($createdAt -ge $Cutoff) { Add-Content $LogFile "SKIPPED  run=$($p.RunId) newer than cutoff"; continue }
    if ($LongLivedBranches -contains $p.Branch -and $createdAt -ge $MainCutoff) { Add-Content $LogFile "SKIPPED  run=$($p.RunId) $($p.Branch), newer than main/gh-pages cutoff"; continue }
    if (Invoke-GhDelete "repos/$Repo/actions/runs/$($p.RunId)") { $deleted++; Add-Content $LogFile "deleted  run=$($p.RunId)  $($p.Workflow)  $($p.Branch)  $($p.Reason)" }
    else                                                      { $failed++;  Add-Content $LogFile "FAILED   run=$($p.RunId)  $($p.Workflow)  $($p.Branch)  $($p.Reason)" }
}

# 8. Delete caches
$cacheDeleted = 0; $cacheFailed = 0
foreach ($c in $cachePlan) {
    if (Invoke-GhDelete "repos/$Repo/actions/caches/$($c.CacheId)") { $cacheDeleted++; Add-Content $LogFile "deleted  cache=$($c.CacheId)  $($c.Ref)  $($c.Key)  $($c.Reason)" }
    else                                                           { $cacheFailed++;  Add-Content $LogFile "FAILED   cache=$($c.CacheId)  $($c.Ref)  $($c.Key)  $($c.Reason)" }
}

$summary = "Deleted $deleted workflow runs (failures: $failed) and $cacheDeleted caches (failures: $cacheFailed)."
Add-Content $LogFile $summary
Write-Host $summary
Write-Host "Log: $LogFile"
Write-Summary ''
Write-Summary "**$summary**"

if ($failed -gt 0 -or $cacheFailed -gt 0) { exit 1 }
