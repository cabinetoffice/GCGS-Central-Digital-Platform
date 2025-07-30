using CO.CDP.OrganisationApp.Pages.Forms;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementModelTests
{
    private class TestFormElementModel : FormElementModel
    {
        public override Models.FormAnswer GetAnswer()
        {
            throw new NotImplementedException();
        }

        public override void SetAnswer(Models.FormAnswer? answer)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void GetFieldName_WithQuestionId_ReturnsPrefixedName()
    {
        var model = new TestFormElementModel { QuestionId = Guid.NewGuid() };
        var propertyName = "TestProperty";
        var expected = $"Q_{model.QuestionId.Value}_{propertyName}";

        var result = model.GetFieldName(propertyName);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFieldName_WithoutQuestionId_ReturnsOriginalName()
    {
        var model = new TestFormElementModel { QuestionId = null };
        var propertyName = "TestProperty";
        var expected = propertyName;

        var result = model.GetFieldName(propertyName);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFieldName_WithNullableProperty_ReturnsPrefixedName()
    {
        var model = new TestFormElementModel { QuestionId = Guid.NewGuid() };
        var propertyName = "TestNullableProperty";
        var expected = $"Q_{model.QuestionId.Value}_{propertyName}";

        var result = model.GetFieldName(propertyName);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetFieldName_WithNullableProperty_ReturnsOriginalName()
    {
        var model = new TestFormElementModel { QuestionId = null };
        var propertyName = "TestNullableProperty";
        var expected = propertyName;

        var result = model.GetFieldName(propertyName);

        Assert.Equal(expected, result);
    }
}