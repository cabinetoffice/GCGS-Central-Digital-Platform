using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class MultiQuestionFormElementModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var model = bindingContext.Model as FormElementMultiQuestionModel;

        if (model == null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        foreach (var questionModel in model.QuestionModels)
        {
            switch (questionModel)
            {
                case FormElementTextInputModel textInputModel:
                    BindTextInputModel(textInputModel, bindingContext);
                    break;
                case FormElementYesNoInputModel yesNoInputModel:
                    BindYesNoInputModel(yesNoInputModel, bindingContext);
                    break;
                case FormElementDateInputModel dateInputModel:
                    BindDateInputModel(dateInputModel, bindingContext);
                    break;
                case FormElementSingleChoiceModel singleChoiceModel:
                    BindSingleChoiceModel(singleChoiceModel, bindingContext);
                    break;
                case FormElementGroupedSingleChoiceModel groupedSingleChoiceModel:
                    BindGroupedSingleChoiceModel(groupedSingleChoiceModel, bindingContext);
                    break;
                case FormElementMultiLineInputModel multiLineInputModel:
                    BindMultiLineInputModel(multiLineInputModel, bindingContext);
                    break;
                case FormElementUrlInputModel urlInputModel:
                    BindUrlInputModel(urlInputModel, bindingContext);
                    break;
                case FormElementAddressModel addressModel:
                    BindAddressModel(addressModel, bindingContext);
                    break;
                case FormElementCheckBoxInputModel checkBoxInputModel:
                    BindCheckBoxInputModel(checkBoxInputModel, bindingContext);
                    break;
                case FormElementFileUploadModel fileUploadModel:
                    BindFileUploadModel(fileUploadModel, bindingContext);
                    break;
                case FormElementNoInputModel noInputModel:
                    break;
            }

            if (questionModel is FormElementModel fem)
            {
                bindingContext.ModelState.SetModelValue(fem.GetFieldName("ValidationEntry"), "", "");
            }
        }

        bindingContext.Result = ModelBindingResult.Success(model);
        return Task.CompletedTask;
    }

    private static void BindTextInputModel(FormElementTextInputModel model, ModelBindingContext bindingContext)
    {
        var textInputResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.TextInput)));
        if (textInputResult != ValueProviderResult.None)
        {
            model.TextInput = textInputResult.FirstValue;
        }

        var hasValueResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.HasValue)));
        if (hasValueResult != ValueProviderResult.None && !string.IsNullOrEmpty(hasValueResult.FirstValue))
        {
            model.HasValue = bool.Parse(hasValueResult.FirstValue);
        }
        else
        {
            model.HasValue = null;
        }
    }

    private static void BindYesNoInputModel(FormElementYesNoInputModel model, ModelBindingContext bindingContext)
    {
        var yesNoInputResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.YesNoInput)));
        if (yesNoInputResult != ValueProviderResult.None)
        {
            model.YesNoInput = yesNoInputResult.FirstValue;
        }
    }

    private static void BindDateInputModel(FormElementDateInputModel model, ModelBindingContext bindingContext)
    {
        var dayResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.Day)));
        if (dayResult != ValueProviderResult.None)
        {
            model.Day = dayResult.FirstValue;
        }

        var monthResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.Month)));
        if (monthResult != ValueProviderResult.None)
        {
            model.Month = monthResult.FirstValue;
        }

        var yearResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.Year)));
        if (yearResult != ValueProviderResult.None)
        {
            model.Year = yearResult.FirstValue;
        }

        var hasValueResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.HasValue)));
        if (hasValueResult != ValueProviderResult.None && !string.IsNullOrEmpty(hasValueResult.FirstValue))
        {
            model.HasValue = bool.Parse(hasValueResult.FirstValue);
        }
        else
        {
            model.HasValue = null;
        }
    }

    private static void BindSingleChoiceModel(FormElementSingleChoiceModel model, ModelBindingContext bindingContext)
    {
        var selectedOptionResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.SelectedOption)));
        if (selectedOptionResult != ValueProviderResult.None)
        {
            model.SelectedOption = selectedOptionResult.FirstValue;
        }
    }

    private static void BindGroupedSingleChoiceModel(FormElementGroupedSingleChoiceModel model, ModelBindingContext bindingContext)
    {
        var selectedOptionResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.SelectedOption)));
        if (selectedOptionResult != ValueProviderResult.None)
        {
            model.SelectedOption = selectedOptionResult.FirstValue;
        }
    }

    private static void BindMultiLineInputModel(FormElementMultiLineInputModel model, ModelBindingContext bindingContext)
    {
        var textInputResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.TextInput)));
        if (textInputResult != ValueProviderResult.None)
        {
            model.TextInput = textInputResult.FirstValue;
        }
    }

    private static void BindUrlInputModel(FormElementUrlInputModel model, ModelBindingContext bindingContext)
    {
        var textInputResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.TextInput)));
        if (textInputResult != ValueProviderResult.None)
        {
            model.TextInput = textInputResult.FirstValue;
        }

        var hasValueResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.HasValue)));
        if (hasValueResult != ValueProviderResult.None && !string.IsNullOrEmpty(hasValueResult.FirstValue))
        {
            model.HasValue = bool.Parse(hasValueResult.FirstValue);
        }
        else
        {
            model.HasValue = null;
        }
    }

    private static void BindAddressModel(FormElementAddressModel model, ModelBindingContext bindingContext)
    {
        var addressLine1Result = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.AddressLine1)));
        if (addressLine1Result != ValueProviderResult.None)
        {
            model.AddressLine1 = addressLine1Result.FirstValue;
        }

        var townOrCityResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.TownOrCity)));
        if (townOrCityResult != ValueProviderResult.None)
        {
            model.TownOrCity = townOrCityResult.FirstValue;
        }

        var postcodeResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.Postcode)));
        if (postcodeResult != ValueProviderResult.None)
        {
            model.Postcode = postcodeResult.FirstValue;
        }

        var countryResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.Country)));
        if (countryResult != ValueProviderResult.None)
        {
            model.Country = countryResult.FirstValue;
        }
    }

    private static void BindCheckBoxInputModel(FormElementCheckBoxInputModel model, ModelBindingContext bindingContext)
    {
        var checkBoxInputResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.CheckBoxInput)));
        if (checkBoxInputResult != ValueProviderResult.None && !string.IsNullOrEmpty(checkBoxInputResult.FirstValue))
        {
            model.CheckBoxInput = bool.Parse(checkBoxInputResult.FirstValue);
        }
        else
        {
            model.CheckBoxInput = null;
        }
    }

    private static void BindFileUploadModel(FormElementFileUploadModel model, ModelBindingContext bindingContext)
    {
        var uploadedFileNameResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.UploadedFileName)));
        if (uploadedFileNameResult != ValueProviderResult.None)
        {
            model.UploadedFileName = uploadedFileNameResult.FirstValue;
        }

        var hasValueResult = bindingContext.ValueProvider.GetValue(model.GetFieldName(nameof(model.HasValue)));
        if (hasValueResult != ValueProviderResult.None && !string.IsNullOrEmpty(hasValueResult.FirstValue))
        {
            model.HasValue = bool.Parse(hasValueResult.FirstValue);
        }
        else
        {
            model.HasValue = null;
        }
    }
}