using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class MultiQuestionFormElementModelBinderTests
{
    private readonly MultiQuestionFormElementModelBinder _binder = new();
    private readonly EmptyModelMetadataProvider _metadataProvider = new();

    private ModelBindingContext CreateBindingContext(FormElementMultiQuestionModel model, IFormCollection formCollection)
    {
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Method = "POST",
                ContentType = "application/x-www-form-urlencoded",
                Form = formCollection
            }
        };

        var modelMetadata = _metadataProvider.GetMetadataForType(typeof(FormElementMultiQuestionModel));

        var bindingContext = new DefaultModelBindingContext
        {
            ActionContext = new Microsoft.AspNetCore.Mvc.ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()),
            Model = model,
            ModelMetadata = modelMetadata,
            ModelState = new ModelStateDictionary(),
            ValueProvider = new FormValueProvider(BindingSource.Form, formCollection, System.Globalization.CultureInfo.CurrentCulture),
            ValidationState = new ValidationStateDictionary()
        };

        return bindingContext;
    }

    [Fact]
    public async Task BindModelAsync_BindsTextInputModelCorrectly()
    {
        var questionId = Guid.NewGuid();
        var formQuestion = new FormQuestion { Id = questionId, Type = FormQuestionType.Text };
        var multiQuestionPageModel = new MultiQuestionPageModel { Questions = new List<FormQuestion> { formQuestion } };

        var multiQuestionModel = new FormElementMultiQuestionModel();
        multiQuestionModel.Initialize(multiQuestionPageModel, new Dictionary<Guid, FormAnswer>());

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { $"Q_{questionId}_TextInput", "Test Value" },
            { $"Q_{questionId}_HasValue", "true" }
        });

        var bindingContext = CreateBindingContext(multiQuestionModel, formCollection);

        await _binder.BindModelAsync(bindingContext);

        Assert.True(bindingContext.Result.IsModelSet);
        var boundModel = Assert.IsType<FormElementMultiQuestionModel>(bindingContext.Result.Model);
        var boundTextInputModel = Assert.IsType<FormElementTextInputModel>(boundModel.QuestionModels.First());

        Assert.Equal("Test Value", boundTextInputModel.TextInput);
        Assert.True(boundTextInputModel.HasValue);
    }

    [Fact]
    public async Task BindModelAsync_BindsYesNoInputModelCorrectly()
    {
        var questionId = Guid.NewGuid();
        var formQuestion = new FormQuestion { Id = questionId, Type = FormQuestionType.YesOrNo };
        var multiQuestionPageModel = new MultiQuestionPageModel { Questions = new List<FormQuestion> { formQuestion } };

        var multiQuestionModel = new FormElementMultiQuestionModel();
        multiQuestionModel.Initialize(multiQuestionPageModel, new Dictionary<Guid, FormAnswer>());

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { $"Q_{questionId}_YesNoInput", "yes" }
        });

        var bindingContext = CreateBindingContext(multiQuestionModel, formCollection);

        await _binder.BindModelAsync(bindingContext);

        Assert.True(bindingContext.Result.IsModelSet);
        var boundModel = Assert.IsType<FormElementMultiQuestionModel>(bindingContext.Result.Model);
        var boundYesNoInputModel = Assert.IsType<FormElementYesNoInputModel>(boundModel.QuestionModels.First());

        Assert.Equal("yes", boundYesNoInputModel.YesNoInput);
    }

    [Fact]
    public async Task BindModelAsync_BindsDateInputModelCorrectly()
    {
        var questionId = Guid.NewGuid();
        var formQuestion = new FormQuestion { Id = questionId, Type = FormQuestionType.Date };
        var multiQuestionPageModel = new MultiQuestionPageModel { Questions = new List<FormQuestion> { formQuestion } };

        var multiQuestionModel = new FormElementMultiQuestionModel();
        multiQuestionModel.Initialize(multiQuestionPageModel, new Dictionary<Guid, FormAnswer>());

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { $"Q_{questionId}_Day", "15" },
            { $"Q_{questionId}_Month", "6" },
            { $"Q_{questionId}_Year", "2023" },
            { $"Q_{questionId}_HasValue", "true" }
        });

        var bindingContext = CreateBindingContext(multiQuestionModel, formCollection);

        await _binder.BindModelAsync(bindingContext);

        Assert.True(bindingContext.Result.IsModelSet);
        var boundModel = Assert.IsType<FormElementMultiQuestionModel>(bindingContext.Result.Model);
        var boundDateInputModel = Assert.IsType<FormElementDateInputModel>(boundModel.QuestionModels.First());

        Assert.Equal("15", boundDateInputModel.Day);
        Assert.Equal("6", boundDateInputModel.Month);
        Assert.Equal("2023", boundDateInputModel.Year);
        Assert.True(boundDateInputModel.HasValue);
    }

    [Fact]
    public async Task BindModelAsync_BindsSingleChoiceModelCorrectly()
    {
        var questionId = Guid.NewGuid();
        var formQuestion = new FormQuestion { Id = questionId, Type = FormQuestionType.SingleChoice };
        var multiQuestionPageModel = new MultiQuestionPageModel { Questions = new List<FormQuestion> { formQuestion } };

        var multiQuestionModel = new FormElementMultiQuestionModel();
        multiQuestionModel.Initialize(multiQuestionPageModel, new Dictionary<Guid, FormAnswer>());

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { $"Q_{questionId}_SelectedOption", "option1" }
        });

        var bindingContext = CreateBindingContext(multiQuestionModel, formCollection);

        await _binder.BindModelAsync(bindingContext);

        Assert.True(bindingContext.Result.IsModelSet);
        var boundModel = Assert.IsType<FormElementMultiQuestionModel>(bindingContext.Result.Model);
        var boundSingleChoiceModel = Assert.IsType<FormElementSingleChoiceModel>(boundModel.QuestionModels.First());

        Assert.Equal("option1", boundSingleChoiceModel.SelectedOption);
    }

    [Fact]
    public async Task BindModelAsync_BindsMultipleQuestionsCorrectly()
    {
        var textQuestionId = Guid.NewGuid();
        var yesNoQuestionId = Guid.NewGuid();

        var textQuestion = new FormQuestion { Id = textQuestionId, Type = FormQuestionType.Text };
        var yesNoQuestion = new FormQuestion { Id = yesNoQuestionId, Type = FormQuestionType.YesOrNo };

        var multiQuestionPageModel = new MultiQuestionPageModel
        {
            Questions = new List<FormQuestion> { textQuestion, yesNoQuestion }
        };

        var multiQuestionModel = new FormElementMultiQuestionModel();
        multiQuestionModel.Initialize(multiQuestionPageModel, new Dictionary<Guid, FormAnswer>());

        var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
        {
            { $"Q_{textQuestionId}_TextInput", "Text Answer" },
            { $"Q_{textQuestionId}_HasValue", "true" },
            { $"Q_{yesNoQuestionId}_YesNoInput", "no" }
        });

        var bindingContext = CreateBindingContext(multiQuestionModel, formCollection);

        await _binder.BindModelAsync(bindingContext);

        Assert.True(bindingContext.Result.IsModelSet);
        var boundModel = Assert.IsType<FormElementMultiQuestionModel>(bindingContext.Result.Model);

        Assert.Equal(2, boundModel.QuestionModels.Count());

        var textInputModel = boundModel.QuestionModels.OfType<FormElementTextInputModel>().First();
        Assert.Equal("Text Answer", textInputModel.TextInput);
        Assert.True(textInputModel.HasValue);

        var yesNoInputModel = boundModel.QuestionModels.OfType<FormElementYesNoInputModel>().First();
        Assert.Equal("no", yesNoInputModel.YesNoInput);
    }

    [Fact]
    public async Task BindModelAsync_HandlesNullModel()
    {
        var bindingContext = CreateBindingContext(null!, new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()));

        await _binder.BindModelAsync(bindingContext);

        Assert.False(bindingContext.Result.IsModelSet);
    }
}