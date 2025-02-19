using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.Localization;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Localization;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using static CO.CDP.DataSharing.WebApi.Tests.DataSharingFactory;

namespace CO.CDP.DataSharing.WebApi.Tests.DataService
{
    public class DataServiceTests
    {
        private readonly Mock<IShareCodeRepository> _shareCodeRepository = new();
        private readonly Mock<IHtmlLocalizer<FormsEngineResource>> _localizer = new();
        private WebApi.DataService.DataService DataService => new(_shareCodeRepository.Object, _localizer.Object);

        public DataServiceTests()
        {
            _localizer.Setup(l => l[It.IsAny<string>()])
                .Returns((string key) =>
                {
                    if (key == "Localized_String")
                    {
                        return new LocalizedHtmlString("Localized_String", "Localized string");
                    }
                    return new LocalizedHtmlString(key, key);
                });
        }

        [Fact]
        public async Task GetSharedSupplierInformationAsync_ShouldReturnSharedSupplierInformation_WhenOrganisationExists()
        {
            var shareCode = "ABC-123";
            var sharedConsent = CreateSharedConsent(shareCode: shareCode);
            _shareCodeRepository.Setup(r => r.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

            var result = await DataService.GetSharedSupplierInformationAsync(shareCode);

            result.Should().NotBeNull();
            result.BasicInformation?.SupplierType.Should().Be(sharedConsent.Organisation.SupplierInfo?.SupplierType);
        }

        [Fact]
        public async Task GetSharedSupplierInformationAsync_ShouldPopulateBasicInformationOrgName_WhenSupplierInfoIsNull()
        {
            var shareCode = "ABC-123";
            var sharedConsent = CreateSharedConsent(shareCode: shareCode);
            sharedConsent.Organisation.SupplierInfo = null;

            _shareCodeRepository.Setup(r => r.GetByShareCode(shareCode)).ReturnsAsync(sharedConsent);

            var result = await DataService.GetSharedSupplierInformationAsync(shareCode);

            result.Should().NotBeNull();
            result.BasicInformation.Should().NotBeNull();
            result.BasicInformation!.OrganisationName.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldThrowShareCodeNotFoundException_WhenShareCodeDoesNotExist()
        {
            var shareCode = "invalid-sharecode";
            _shareCodeRepository.Setup(repo => repo.GetByShareCode(shareCode))
                .ReturnsAsync((SharedConsentNonEf?)null);

            Func<Task> act = async () => await DataService.GetSharedSupplierInformationAsync(shareCode);

            await act.Should().ThrowAsync<ShareCodeNotFoundException>();
        }

        [Fact]
        public async Task ShouldMapOrganisationToBasicInformation()
        {
            var sharedConsent = CreateSharedConsent();
            var organisation = sharedConsent.Organisation;

            _shareCodeRepository.Setup(r => r.GetByShareCode("ABC-123")).ReturnsAsync(sharedConsent);

            var result = await DataService.GetSharedSupplierInformationAsync("ABC-123");

            result.BasicInformation!.SupplierType.Should().Be(organisation.SupplierInfo?.SupplierType);
            result.BasicInformation.RegisteredAddress.Should().NotBeNull();
            result.BasicInformation.PostalAddress.Should().NotBeNull();
            result.BasicInformation.VatNumber.Should()
                .Be(organisation.Identifiers.FirstOrDefault(i => i.Scheme == "VAT")?.IdentifierId);
            result.BasicInformation.WebsiteAddress.Should().Be(organisation.ContactPoints.FirstOrDefault()?.Url);
            result.BasicInformation.EmailAddress.Should().Be(organisation.ContactPoints.FirstOrDefault()?.Email);
            result.BasicInformation.LegalForm.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldMapBasicInformation_WhenAllCompleteFlagsFalse()
        {
            var sharedConsent = CreateSharedConsent("ALL-FALSE");
            var supplierInfo = sharedConsent.Organisation.SupplierInfo!;
            supplierInfo.CompletedRegAddress = false;
            supplierInfo.CompletedPostalAddress = false;
            supplierInfo.CompletedVat = false;
            supplierInfo.CompletedWebsiteAddress = false;
            supplierInfo.CompletedEmailAddress = false;
            supplierInfo.CompletedLegalForm = false;
            sharedConsent.Organisation.Roles.Clear();
            sharedConsent.Organisation.Roles.Add(PartyRole.Buyer);

            _shareCodeRepository.Setup(r => r.GetByShareCode("ALL-FALSE"))
                .ReturnsAsync(sharedConsent);

            var result = await DataService.GetSharedSupplierInformationAsync("ALL-FALSE");
            result.BasicInformation.RegisteredAddress.Should().BeNull();
            result.BasicInformation.PostalAddress.Should().BeNull();
            result.BasicInformation.VatNumber.Should().BeNull();
            result.BasicInformation.WebsiteAddress.Should().BeNull();
            result.BasicInformation.EmailAddress.Should().BeNull();
            result.BasicInformation.LegalForm.Should().BeNull();
            result.BasicInformation.Role.Should().Be("Buyer");
        }

        [Fact]
        public async Task ShouldMapFormAnswerSetsForPdf()
        {
            var org = CreateOrganisation();
            var sharedConsent = NonEfEntityFactory.GetSharedConsent(org.Guid, Guid.NewGuid());
            sharedConsent.Organisation = org;

            var exclusionAnswerSet = sharedConsent.AnswerSets.FirstOrDefault();
            if (exclusionAnswerSet != null)
            {
                var firstAnswer = exclusionAnswerSet.Answers.FirstOrDefault();
                if (firstAnswer != null)
                {
                    firstAnswer.Question.Type = OrganisationInformation.Persistence.Forms.FormQuestionType.GroupedSingleChoice;
                    firstAnswer.OptionValue = "acting_improperly";
                }
            }

            _shareCodeRepository.Setup(r => r.GetByShareCode("ABC-123")).ReturnsAsync(sharedConsent);
            var result = await DataService.GetSharedSupplierInformationAsync("ABC-123");

            result.FormAnswerSetForPdfs.Should().ContainSingle();
            var formSection = result.FormAnswerSetForPdfs.First();
            formSection.SectionName.Should().NotBeNullOrEmpty();
            var groupedChoiceTuple = formSection.QuestionAnswers
                .FirstOrDefault(qa => qa.Item2 == "Acting improperly in procurement");
            groupedChoiceTuple.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldMapAdditionalIdentifiers_ForMultipleIdentifiers()
        {
            var sharedConsent = CreateSharedConsent();
            sharedConsent.Organisation.Identifiers.Add(new IdentifierNonEf
            {
                Scheme = "SOME",
                IdentifierId = "SOME123",
                LegalName = "Some Legal",
                Primary = false
            });
            sharedConsent.Organisation.Identifiers.Add(new IdentifierNonEf
            {
                Scheme = "VAT",
                IdentifierId = "VAT999",
                LegalName = "Vat Legal",
                Primary = false
            });

            _shareCodeRepository.Setup(r => r.GetByShareCode("XYZ-987")).ReturnsAsync(sharedConsent);
            var result = await DataService.GetSharedSupplierInformationAsync("XYZ-987");

            result.AdditionalIdentifiers.Should().Contain(i => i.Id == "SOME123" && i.Scheme == "SOME");
            result.AdditionalIdentifiers.Should().Contain(i => i.Id == "VAT999" && i.Scheme == "VAT");
        }

        [Fact]
        public async Task ShouldMapAttachedDocuments_ForFileUploadAnswers()
        {
            var sharedConsent = CreateSharedConsent(shareCode: "DOC-111");
            if (!sharedConsent.AnswerSets.Any())
            {
                sharedConsent.AnswerSets.Add(new FormAnswerSetNonEf
                {
                    Id = 1,
                    Guid = Guid.NewGuid(),
                    SectionId = 99,
                    Section = new FormSectionNonEf
                    {
                        Id = 99,
                        Title = "MySection",
                        Type = OrganisationInformation.Persistence.Forms.FormSectionType.Exclusions,
                        Questions = new List<FormQuestionNonEf>()
                    },
                    Answers = new List<FormAnswerNonEf>()
                });
            }
            var firstAnswerSet = sharedConsent.AnswerSets.First();
            if (!firstAnswerSet.Answers.Any())
            {
                firstAnswerSet.Answers.Add(new FormAnswerNonEf
                {
                    QuestionId = 42,
                    FormAnswerSetId = firstAnswerSet.Id,
                    Question = new FormQuestionNonEf
                    {
                        Id = 42,
                        Guid = Guid.NewGuid(),
                        SortOrder = 1,
                        Type = OrganisationInformation.Persistence.Forms.FormQuestionType.FileUpload,
                        IsRequired = false,
                        Name = "FileQuestion",
                        Title = "Upload Document",
                        Description = "",
                        Options = new OrganisationInformation.Persistence.Forms.FormQuestionOptions(),
                        Section = firstAnswerSet.Section
                    },
                    FormAnswerSet = firstAnswerSet,
                    TextValue = "mydoc.pdf"
                });
            }

            _shareCodeRepository.Setup(r => r.GetByShareCode("DOC-111")).ReturnsAsync(sharedConsent);
            var result = await DataService.GetSharedSupplierInformationAsync("DOC-111");
            result.AttachedDocuments.Should().ContainSingle().Which.Should().Be("mydoc.pdf");
        }

        [Fact]
        public async Task ShouldMapConnectedPersonsInformation()
        {
            var sharedConsent = CreateSharedConsent(shareCode: "PERSON-123");
            var entity = new ConnectedEntityNonEf
            {
                Guid = Guid.NewGuid(),
                EntityType = OrganisationInformation.ConnectedEntityType.Individual,
                IndividualOrTrust = new ConnectedIndividualTrustNonEf
                {
                    Category = OrganisationInformation.Persistence.ConnectedEntity.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndiv,
                    FirstName = "Jane",
                    LastName = "Doe"
                },
                Addresses = [
                    new AddressNonEf
                    {
                        StreetAddress = "10 Downing St",
                        Locality = "London",
                        CountryName = "UK",
                        Country = "GB",
                        Type = OrganisationInformation.AddressType.Registered
                    }
                ]
            };
            sharedConsent.Organisation.ConnectedEntities.Add(entity);

            _shareCodeRepository.Setup(r => r.GetByShareCode("PERSON-123")).ReturnsAsync(sharedConsent);
            var result = await DataService.GetSharedSupplierInformationAsync("PERSON-123");
            result.ConnectedPersonInformation.Should().NotBeEmpty();
            var person = result.ConnectedPersonInformation.Last();
            person.FirstName.Should().Be("Jane");
            person.LastName.Should().Be("Doe");
            person.Addresses.Should().ContainSingle();
            person.Addresses.First().StreetAddress.Should().Be("10 Downing St");
        }

        [Fact]
        public async Task ShouldUseUnknownTitle_WhenOptionValueIsNotRecognized()
        {
            var org = CreateOrganisation();
            var sharedConsent = NonEfEntityFactory.GetSharedConsent(org.Guid, Guid.NewGuid());
            sharedConsent.Organisation = org;
            var answerSet = sharedConsent.AnswerSets.FirstOrDefault();
            if (answerSet != null)
            {
                var answer = answerSet.Answers.FirstOrDefault();
                if (answer != null)
                {
                    answer.Question.Type = OrganisationInformation.Persistence.Forms.FormQuestionType.GroupedSingleChoice;
                    answer.OptionValue = "unrecognized_code";
                }
            }

            _shareCodeRepository.Setup(r => r.GetByShareCode("UNKNOWN-321")).ReturnsAsync(sharedConsent);
            var result = await DataService.GetSharedSupplierInformationAsync("UNKNOWN-321");
            var section = result.FormAnswerSetForPdfs.First();
            section.QuestionAnswers.Should().Contain(qa => qa.Item2 == "Unknown");
        }

        [Fact]
        public async Task ShouldMapYesNoAnswerFalse_ResultsInNotSpecified()
        {
            var org = CreateOrganisation();
            var sharedConsent = NonEfEntityFactory.GetSharedConsent(org.Guid, Guid.NewGuid());
            sharedConsent.Organisation = org;
            var answerSet = sharedConsent.AnswerSets.FirstOrDefault();
            if (answerSet != null)
            {
                var answer = answerSet.Answers.FirstOrDefault();
                if (answer != null)
                {
                    answer.Question.Type = OrganisationInformation.Persistence.Forms.FormQuestionType.YesOrNo;
                    answer.BoolValue = false;
                    answer.OptionValue = null;
                }
            }

            _shareCodeRepository.Setup(r => r.GetByShareCode("BOOL-FALSE")).ReturnsAsync(sharedConsent);
            var result = await DataService.GetSharedSupplierInformationAsync("BOOL-FALSE");
            var section = result.FormAnswerSetForPdfs.First();
            section.QuestionAnswers.Should().Contain(qa => qa.Item2 == "Not specified");
        }

        [Fact]
        public async Task ShouldMapYesNoAnswerTrue_WithoutOptionValue_ResultsInYes()
        {
            var org = CreateOrganisation();
            var sharedConsent = NonEfEntityFactory.GetSharedConsent(org.Guid, Guid.NewGuid());
            sharedConsent.Organisation = org;

            var firstAnswerSet = sharedConsent.AnswerSets.FirstOrDefault();
            if (firstAnswerSet?.Answers.Any() == true)
            {
                var yesNoAnswer = firstAnswerSet.Answers.First();
                yesNoAnswer.Question.Type = OrganisationInformation.Persistence.Forms.FormQuestionType.YesOrNo;
                yesNoAnswer.BoolValue = true;
                yesNoAnswer.OptionValue = null;
            }

            _shareCodeRepository.Setup(r => r.GetByShareCode("BOOL-TRUE")).ReturnsAsync(sharedConsent);
            var result = await DataService.GetSharedSupplierInformationAsync("BOOL-TRUE");
            var section = result.FormAnswerSetForPdfs.First();
            section.QuestionAnswers.Should().Contain(qa => qa.Item2 == "Yes");
        }

        [Fact]
        public async Task ShouldMapYesNoAnswerTrue_WithOptionValue_UsesOptionValueInstead()
        {
            var org = CreateOrganisation();
            var sharedConsent = NonEfEntityFactory.GetSharedConsent(org.Guid, Guid.NewGuid());
            sharedConsent.Organisation = org;

            var firstAnswerSet = sharedConsent.AnswerSets.FirstOrDefault();
            if (firstAnswerSet?.Answers.Any() == true)
            {
                var yesNoAnswer = firstAnswerSet.Answers.First();
                yesNoAnswer.Question.Type = OrganisationInformation.Persistence.Forms.FormQuestionType.YesOrNo;
                yesNoAnswer.BoolValue = true;
                yesNoAnswer.OptionValue = "OverrideValue";
            }

            _shareCodeRepository.Setup(r => r.GetByShareCode("BOOL-OVERRIDE")).ReturnsAsync(sharedConsent);
            var result = await DataService.GetSharedSupplierInformationAsync("BOOL-OVERRIDE");
            var section = result.FormAnswerSetForPdfs.First();

            section.QuestionAnswers.Should().Contain(qa => qa.Item2 == "OverrideValue");
        }

        [Fact]
        public async Task ShouldReturnBuyer_WhenRolesDoNotContainTenderer()
        {
            var sharedConsent = CreateSharedConsent("BUYER-ONLY");
            sharedConsent.Organisation.Roles.Clear();
            sharedConsent.Organisation.Roles.Add(PartyRole.Buyer);

            _shareCodeRepository.Setup(r => r.GetByShareCode("BUYER-ONLY"))
                .ReturnsAsync(sharedConsent);

            var result = await DataService.GetSharedSupplierInformationAsync("BUYER-ONLY");
            result.BasicInformation.Role.Should().Be("Buyer");
        }
    }
}