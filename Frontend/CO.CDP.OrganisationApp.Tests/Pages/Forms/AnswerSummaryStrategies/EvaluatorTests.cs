using System;
using System.Collections.Generic;
using System.Linq;
using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms.AnswerSummaryStrategies;
using FluentAssertions;
using Xunit;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms.AnswerSummaryStrategies;
public class EvaluatorTests
{
    [Fact]
    public void EvaluatorFactory_ShouldResolveRegisteredEvaluator()
    {
        var evaluators = new IEvaluator[] { new StringFormatEvaluator(), new EqualityEvaluator() };
        var factory = new EvaluatorFactory(evaluators);

        var evaluator = factory.CreateEvaluator(ExpressionOperationType.StringFormat);
        evaluator.Should().BeOfType<StringFormatEvaluator>();
    }

    [Fact]
    public void EvaluatorFactory_ShouldThrowForUnregisteredEvaluator()
    {
        var factory = new EvaluatorFactory([]);

        Action act = () => factory.CreateEvaluator(ExpressionOperationType.StringFormat);

        act.Should().Throw<NotSupportedException>()
            .WithMessage("No evaluator registered for: StringFormat");
    }

    [Theory]
    [InlineData("Hello {0}", new object[] { "World" }, "Hello World")]
    [InlineData("{0} + {1} = {2}", new object[] { 2, 3, 5 }, "2 + 3 = 5")]
    public void StringFormatEvaluator_ShouldFormatCorrectly(string expression, object[] parameters, string expected)
    {
        var evaluator = new StringFormatEvaluator();

        var result = evaluator.Evaluate(expression, parameters);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\"Yes\"==\"Yes\"?\"Match\":\"No Match\"", "Match")]
    [InlineData("\"abc\"==\"xyz\"?\"True\":\"False\"", "False")]
    public void EqualityEvaluator_ShouldReturnExpectedResult(string expression, string expected)
    {
        var evaluator = new EqualityEvaluator();

        var result = evaluator.Evaluate(expression, []);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("\"Yes\"=?\"Yes\":\"No\"")]
    [InlineData("\"Yes\"==\"Yes\":\"No\"")]
    public void EqualityEvaluator_ShouldThrowOnInvalidFormat(string expression)
    {
        var evaluator = new EqualityEvaluator();

        Action act = () => evaluator.Evaluate(expression, []);

        act.Should().Throw<FormatException>();
    }    
}