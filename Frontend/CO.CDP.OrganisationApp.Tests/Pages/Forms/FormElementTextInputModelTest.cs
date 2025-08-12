using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementTextInputModelTest
{
    private readonly FormElementTextInputModel _model = new();

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("Test value")]
    public void GetAnswer_WhenHasValueFalse_GetsExpectedFormAnswer(string? input)
    {
        _model.HasValue = false;
        _model.TextInput = input;

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().Be(false);
        answer.As<FormAnswer>().TextValue.Should().BeNull();
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("Test value", "Test value")]
    public void GetAnswer_WhenHasValueTrue_GetsExpectedFormAnswer(string? input, string? expected)
    {
        _model.HasValue = true;
        _model.TextInput = input;

        var answer = _model.GetAnswer();

        answer.Should().NotBeNull();
        answer.As<FormAnswer>().BoolValue.Should().Be(true);
        answer.As<FormAnswer>().TextValue.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(" ", null)]
    [InlineData("Test value", "Test value")]
    public void GetAnswer_WhenHasValueIsNull_GetsExpectedFormAnswer(string? input, string? expected)
    {
        _model.HasValue = null;
        _model.TextInput = input;

        var answer = _model.GetAnswer();

        if (expected == null)
        {
            answer.Should().BeNull();
        }
        else
        {
            answer.Should().NotBeNull();
            answer.As<FormAnswer>().BoolValue.Should().BeNull();
            answer.As<FormAnswer>().TextValue.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("Test value", "Test value")]
    public void SetAnswer_SetsExpectedTextInput(string? input, string? expected)
    {
        var answer = new FormAnswer { TextValue = input };

        _model.SetAnswer(answer);

        if (expected == null)
        {
            _model.TextInput.Should().BeNull();
        }
        else
        {
            _model.TextInput.Should().NotBeNull();
            _model.TextInput!.Should().Be(expected);
        }
    }

    [Theory]
    [InlineData(true, null, null, "Enter a value")]
    [InlineData(true, null, " ", "Enter a value")]
    [InlineData(false, false, null, null)]
    [InlineData(false, true, "Some value", null)]
    public void Validate_ReturnsExpectedResults(bool isRequired, bool? hasValue, string? textInput, string? expectedErrorMessage)
    {
        _model.IsRequired = isRequired;
        _model.HasValue = hasValue;
        _model.CurrentFormQuestionType = FormQuestionType.Text;
        _model.TextInput = textInput;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        if (expectedErrorMessage != null)
        {
            validationResults.Should().ContainSingle();
            validationResults.First().ErrorMessage.Should().Be(expectedErrorMessage);
        }
        else
        {
            validationResults.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData("invalid-email", "Enter an email address in the correct format, like name@example.com")]
    [InlineData("valid.email@example.com", null)]
    public void Validate_EmailValidation_ReturnsExpectedResults(string? email, string? expectedErrorMessage)
    {
        _model.IsRequired = true;
        _model.CurrentFormQuestionType = FormQuestionType.Text;
        _model.Heading = "Email Address";
        _model.TextInput = email;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        if (expectedErrorMessage != null)
        {
            validationResults.Should().ContainSingle();
            validationResults.First().ErrorMessage.Should().Be(expectedErrorMessage);
        }
        else
        {
            validationResults.Should().BeEmpty();
        }
    }

    [Theory]
    [InlineData(true, null, "Email Address", null, "Enter an email address in the correct format, like name@example.com")]
    [InlineData(true, null, "Email Address", "invalid-email", "Enter an email address in the correct format, like name@example.com")]
    [InlineData(true, null, "Email Address", "valid.email@example.com", null)]
    [InlineData(false, true, "Email Address", "somevalue", "Enter an email address in the correct format, like name@example.com")]
    [InlineData(false, false, "Email Address", null, null)]
    [InlineData(false, true, "Name", "invalid-email", null)]
    public void Validate_EmailFieldValidationBasedOnHeading_ReturnsExpectedResults(bool isRequired, bool? hasValue, string heading, string? textInput, string? expectedErrorMessage)
    {
        _model.IsRequired = isRequired;
        _model.HasValue = hasValue;
        _model.CurrentFormQuestionType = FormQuestionType.Text;
        _model.Heading = heading;
        _model.TextInput = textInput;

        var validationResults = _model.Validate(new ValidationContext(_model)).ToList();

        if (expectedErrorMessage != null)
        {
            validationResults.Should().ContainSingle();
            validationResults.First().ErrorMessage.Should().Be(expectedErrorMessage);
        }
        else
        {
            validationResults.Should().BeEmpty();
        }
    }

    private FormElementTextInputModel CreateModelWithOptions(FormQuestionType type, bool isRequired, InputWidthType? inputWidth = null, InputSuffixOptions? inputSuffix = null, string? customCssClasses = null, TextValidationType? textValidationType = null)
    {
        return new FormElementTextInputModel
        {
            CurrentFormQuestionType = type,
            IsRequired = isRequired,
            Options = new FormQuestionOptions
            {
                Layout = new LayoutOptions
                {
                    Input = new InputOptions
                    {
                        Width = inputWidth,
                        Suffix = inputSuffix,
                        CustomCssClasses = customCssClasses
                    }
                },
                Validation = new ValidationOptions
                {
                    TextValidationType = textValidationType
                }
            }
        };
    }

    [Fact]
    public void Options_InputWidth_IsSet()
    {
        var model = CreateModelWithOptions(FormQuestionType.Text, true, inputWidth: InputWidthType.OneHalf);
        model.Options?.Layout?.Input?.Width.Should().Be(InputWidthType.OneHalf);
    }

    [Fact]
    public void Options_InputSuffix_CustomText_IsSet()
    {
        var inputSuffixOptions = new InputSuffixOptions
        {
            Type = InputSuffixType.CustomText,
            Text = "kg"
        };
        var model = CreateModelWithOptions(FormQuestionType.Text, true, inputSuffix: inputSuffixOptions);
        model.Options?.Layout?.Input?.Suffix.Should().Be(inputSuffixOptions);
    }

    [Fact]
    public void Options_InputSuffix_GovUkDefault_IsSet()
    {
        var inputSuffixOptions = new InputSuffixOptions
        {
            Type = InputSuffixType.GovUkDefault,
            Text = "Global_Percentage"
        };
        var model = CreateModelWithOptions(FormQuestionType.Text, true, inputSuffix: inputSuffixOptions);
        model.Options?.Layout?.Input?.Suffix.Should().Be(inputSuffixOptions);
    }

    [Fact]
    public void Options_CustomCssClasses_IsSet()
    {
        var model = CreateModelWithOptions(FormQuestionType.Text, true, customCssClasses: "custom-class");
        model.Options?.Layout?.Input?.CustomCssClasses.Should().Be("custom-class");
    }

    [Fact]
    public void Validate_YearInputValid_ReturnsNoErrors()
    {
        var model = CreateModelWithOptions(FormQuestionType.Text, true, textValidationType: TextValidationType.Year);
        model.TextInput = "2023";
        var validationContext = new ValidationContext(model);

        var results = model.Validate(validationContext).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_YearInputInvalid_ReturnsError()
    {
        var model = CreateModelWithOptions(FormQuestionType.Text, true, textValidationType: TextValidationType.Year);
        model.TextInput = "invalid";
        var validationContext = new ValidationContext(model);

        var results = model.Validate(validationContext).ToList();

        results.Should().ContainSingle(r => r.MemberNames.Contains("TextInput") && r.ErrorMessage == StaticTextResource.Forms_FormElementTextInput_InvalidYearError);
    }

    [Fact]
    public void Validate_YearInputOutOfRange_ReturnsError()
    {
        var model = CreateModelWithOptions(FormQuestionType.Text, true, textValidationType: TextValidationType.Year);
        model.TextInput = "1899";
        var validationContext = new ValidationContext(model);

        var results = model.Validate(validationContext).ToList();

        results.Should().ContainSingle(r => r.MemberNames.Contains("TextInput") && r.ErrorMessage == StaticTextResource.Forms_FormElementTextInput_InvalidYearError);

        model.TextInput = (DateTimeOffset.UtcNow.Year + 101).ToString();
        results = model.Validate(validationContext).ToList();

        results.Should().ContainSingle(r => r.MemberNames.Contains("TextInput") && r.ErrorMessage == StaticTextResource.Forms_FormElementTextInput_InvalidYearError);
    }

    [Theory]
    [InlineData("123.45", null)]
    [InlineData("0.1", null)]
    [InlineData("1000", null)]
    [InlineData("-123.45", null)]
    [InlineData("0", null)]
    [InlineData("invalid", "Enter a valid decimal number")]
    [InlineData("abc123", "Enter a valid decimal number")]
    [InlineData("12.34.56", "Enter a valid decimal number")]
    public void Validate_DecimalInput_ReturnsExpectedResults(string input, string? expectedErrorMessage)
    {
        var model = CreateModelWithOptions(FormQuestionType.Text, true, textValidationType: TextValidationType.Decimal);
        model.TextInput = input;
        var validationContext = new ValidationContext(model);

        var results = model.Validate(validationContext).ToList();

        if (expectedErrorMessage != null)
        {
            results.Should().ContainSingle(r => r.MemberNames.Contains("TextInput") && r.ErrorMessage == expectedErrorMessage);
        }
        else
        {
            results.Should().BeEmpty();
        }
    }

    [Fact]
    public void Validate_DecimalInputEmpty_ReturnsNoError()
    {
        var model = CreateModelWithOptions(FormQuestionType.Text, false, textValidationType: TextValidationType.Decimal);
        model.TextInput = "";
        model.HasValue = false;
        var validationContext = new ValidationContext(model);

        var results = model.Validate(validationContext).ToList();

        results.Should().BeEmpty();
    }

    [Fact] 
    public void Validate_DecimalInputNull_ReturnsNoError()
    {
        var model = CreateModelWithOptions(FormQuestionType.Text, false, textValidationType: TextValidationType.Decimal);
        model.TextInput = null;
        model.HasValue = false;
        var validationContext = new ValidationContext(model);

        var results = model.Validate(validationContext).ToList();

        results.Should().BeEmpty();
    }
}