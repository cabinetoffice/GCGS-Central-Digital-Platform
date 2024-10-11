using CO.CDP.DataSharing.WebApiClient;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Mime;
using static System.Net.Mime.MediaTypeNames;
using SharedConsentDetails = CO.CDP.OrganisationApp.Models.SharedConsentDetails;

namespace CO.CDP.OrganisationApp.Pages.ShareInformation;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class ShareCodesListViewModel(
    IDataSharingClient dataSharingClient,
    IOrganisationClient organisationClient,
    IFormsClient formsClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty]
    public List<SharedConsentDetails>? SharedConsentDetailsList { get; set; }

    public async Task<IActionResult> OnGet()
    {
        try
        {
            var sharedCodesList = await dataSharingClient.GetShareCodeListAsync(OrganisationId);

            SharedConsentDetailsList = new List<SharedConsentDetails>();

            foreach (var sharedCode in sharedCodesList)
            {
                SharedConsentDetailsList.Add(new SharedConsentDetails
                {
                    ShareCode = sharedCode.ShareCode,
                    SubmittedAt = sharedCode.SubmittedAt
                });
            }
        }
        catch (DataSharing.WebApiClient.ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
        return Page();
    }
    public async Task<IActionResult> OnGetDownload(string shareCode)
    {
        if (string.IsNullOrEmpty(shareCode))
        {
            return Redirect("/page-not-found");
        }

        var fileResponse = await dataSharingClient.GetSharedDataPdfAsync(shareCode);

        if (fileResponse == null)
        {
            return Redirect("/page-not-found");
        }

        var contentDisposition = fileResponse.Headers["Content-Disposition"].FirstOrDefault();
        var filename = string.IsNullOrWhiteSpace(contentDisposition) ? $"{shareCode}.pdf" : new ContentDisposition(contentDisposition).FileName;
        var contentType = fileResponse.Headers["Content-Type"].FirstOrDefault() ?? Application.Pdf;

        return File(fileResponse.Stream, contentType, filename);
    }

    public async Task<bool> MandatorySectionsCompleted()
    {
        try
        {
            var getSupplierInfoTask = organisationClient.GetOrganisationSupplierInformationAsync(OrganisationId);
            var getConnectedEntitiesTask = organisationClient.GetConnectedEntitiesAsync(OrganisationId);
            var formSectionsTask = formsClient.GetFormSectionsAsync(new Guid(FormsEngine.OrganisationSupplierInfoFormId), OrganisationId);

            await Task.WhenAll(getSupplierInfoTask, getConnectedEntitiesTask, formSectionsTask);

            var supplierInfo = getSupplierInfoTask.Result;
            var connectedEntities = getConnectedEntitiesTask.Result;
            var formSections = formSectionsTask.Result.FormSections;

            return SupplierInformationStatus.GetBasicInfoStepStatus(supplierInfo) == SupplierInformationStatus.StepStatus.Completed
                && SupplierInformationStatus.GetConnectedPersonStepStatus(supplierInfo, connectedEntities.Count) == SupplierInformationStatus.StepStatus.Completed
                && formSections
                    .Where(s => s.Type != FormSectionType.Declaration)
                    .All(section => (section.AnswerSetCount != 0 || section.AnswerSetWithFurtherQuestionExemptedExists));
        }
        catch (Exception ex)
            when ((ex is CO.CDP.Organisation.WebApiClient.ApiException oex && oex.StatusCode == 404)
                || (ex is CDP.Forms.WebApiClient.ApiException wex && wex.StatusCode == 404))
        {
            return false;
        }
    }
}