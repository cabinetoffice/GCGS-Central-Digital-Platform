using FluentAssertions;
using CO.CDP.OrganisationApp.Pages.Forms;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementCheckYourAnswersModelTests
{
    [Fact]
    public void GroupConsecutiveSingleQuestions_EmptyList_ReturnsEmpty()
    {
        var result = new List<IAnswerDisplayItem>().GroupConsecutiveSingleQuestions().ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public void GroupConsecutiveSingleQuestions_OnlySingleQuestions_GroupsIntoOne()
    {
        var items = new List<IAnswerDisplayItem>
        {
            new AnswerSummary { Title = "Question 1", Answer = "Answer 1" },
            new AnswerSummary { Title = "Question 2", Answer = "Answer 2" },
            new AnswerSummary { Title = "Question 3", Answer = "Answer 3" }
        };

        var result = items.GroupConsecutiveSingleQuestions().ToList();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<SingleQuestionsGroup>();
        var group = (SingleQuestionsGroup)result[0];
        group.Answers.Should().HaveCount(3);
        group.Answers[0].Title.Should().Be("Question 1");
        group.Answers[1].Title.Should().Be("Question 2");
        group.Answers[2].Title.Should().Be("Question 3");
        items.Should().HaveCount(3);
    }

    [Fact]
    public void GroupConsecutiveSingleQuestions_OnlyGroupedQuestions_ReturnsAsIs()
    {
        var items = new List<IAnswerDisplayItem>
        {
            new GroupedAnswerSummary
            {
                GroupTitle = "Group 1",
                Answers = [new AnswerSummary { Title = "Q1", Answer = "A1" }]
            },
            new GroupedAnswerSummary
            {
                GroupTitle = "Group 2",
                Answers = [new AnswerSummary { Title = "Q2", Answer = "A2" }]
            }
        };

        var result = items.GroupConsecutiveSingleQuestions().ToList();

        result.Should().HaveCount(2);
        result[0].Should().BeOfType<GroupedAnswerSummary>();
        result[1].Should().BeOfType<GroupedAnswerSummary>();
        ((GroupedAnswerSummary)result[0]).GroupTitle.Should().Be("Group 1");
        ((GroupedAnswerSummary)result[1]).GroupTitle.Should().Be("Group 2");
        items.Should().HaveCount(2);
    }

    [Fact]
    public void GroupConsecutiveSingleQuestions_MixedWithConsecutiveSingles_GroupsCorrectly()
    {
        var items = new List<IAnswerDisplayItem>
        {
            new AnswerSummary { Title = "Single 1", Answer = "Answer 1" },
            new AnswerSummary { Title = "Single 2", Answer = "Answer 2" },
            new GroupedAnswerSummary
            {
                GroupTitle = "Multi Group",
                Answers = [new AnswerSummary { Title = "Multi Q", Answer = "Multi A" }]
            },
            new AnswerSummary { Title = "Single 3", Answer = "Answer 3" },
            new AnswerSummary { Title = "Single 4", Answer = "Answer 4" }
        };

        var result = items.GroupConsecutiveSingleQuestions().ToList();

        result.Should().HaveCount(3);

        result[0].Should().BeOfType<SingleQuestionsGroup>();
        var firstGroup = (SingleQuestionsGroup)result[0];
        firstGroup.Answers.Should().HaveCount(2);
        firstGroup.Answers[0].Title.Should().Be("Single 1");
        firstGroup.Answers[1].Title.Should().Be("Single 2");

        result[1].Should().BeOfType<GroupedAnswerSummary>();
        var multiGroup = (GroupedAnswerSummary)result[1];
        multiGroup.GroupTitle.Should().Be("Multi Group");

        result[2].Should().BeOfType<SingleQuestionsGroup>();
        var lastGroup = (SingleQuestionsGroup)result[2];
        lastGroup.Answers.Should().HaveCount(2);
        lastGroup.Answers[0].Title.Should().Be("Single 3");
        lastGroup.Answers[1].Title.Should().Be("Single 4");
        items.Should().HaveCount(5);
    }

    [Fact]
    public void GroupConsecutiveSingleQuestions_AlternatingPattern_EachSingleGetsOwnGroup()
    {
        var items = new List<IAnswerDisplayItem>
        {
            new AnswerSummary { Title = "Single 1", Answer = "Answer 1" },
            new GroupedAnswerSummary
            {
                GroupTitle = "Group 1",
                Answers = [new AnswerSummary { Title = "Q1", Answer = "A1" }]
            },
            new AnswerSummary { Title = "Single 2", Answer = "Answer 2" },
            new GroupedAnswerSummary
            {
                GroupTitle = "Group 2",
                Answers = [new AnswerSummary { Title = "Q2", Answer = "A2" }]
            }
        };

        var result = items.GroupConsecutiveSingleQuestions().ToList();

        result.Should().HaveCount(4);
        result[0].Should().BeOfType<SingleQuestionsGroup>();
        result[1].Should().BeOfType<GroupedAnswerSummary>();
        result[2].Should().BeOfType<SingleQuestionsGroup>();
        result[3].Should().BeOfType<GroupedAnswerSummary>();

        var firstSingle = (SingleQuestionsGroup)result[0];
        firstSingle.Answers.Should().HaveCount(1);
        firstSingle.Answers[0].Title.Should().Be("Single 1");

        var secondSingle = (SingleQuestionsGroup)result[2];
        secondSingle.Answers.Should().HaveCount(1);
        secondSingle.Answers[0].Title.Should().Be("Single 2");
        items.Should().HaveCount(4);
    }

    [Fact]
    public void GroupConsecutiveSingleQuestions_LargeConsecutiveBlock_GroupsAllTogether()
    {
        var items = new List<IAnswerDisplayItem>();
        for (int i = 1; i <= 10; i++)
        {
            items.Add(new AnswerSummary { Title = $"Question {i}", Answer = $"Answer {i}" });
        }

        var result = items.GroupConsecutiveSingleQuestions().ToList();

        result.Should().HaveCount(1);
        result[0].Should().BeOfType<SingleQuestionsGroup>();
        var group = (SingleQuestionsGroup)result[0];
        group.Answers.Should().HaveCount(10);
        group.Answers[0].Title.Should().Be("Question 1");
        group.Answers[9].Title.Should().Be("Question 10");
        items.Should().HaveCount(10);
    }

    [Fact]
    public void GroupedDisplayItems_Property_ReturnsGroupedItems()
    {
        var model = new FormElementCheckYourAnswersModel
        {
            DisplayItems =
            [
                new AnswerSummary { Title = "Single 1", Answer = "Answer 1" },
                new AnswerSummary { Title = "Single 2", Answer = "Answer 2" },
                new GroupedAnswerSummary
                {
                    GroupTitle = "Multi Group",
                    Answers = [new AnswerSummary { Title = "Multi Q", Answer = "Multi A" }]
                }
            ]
        };

        var result = model.GroupedDisplayItems.ToList();

        result.Should().HaveCount(2);
        result[0].Should().BeOfType<SingleQuestionsGroup>();
        result[1].Should().BeOfType<GroupedAnswerSummary>();
    }
}