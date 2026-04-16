--
-- PostgreSQL database dump
--

-- Dumped from database version 16.3 (Debian 16.3-1.pgdg120+1)
-- Dumped by pg_dump version 16.3 (Debian 16.3-1.pgdg120+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20250916152949_AddCpvCodesTable	8.0.8
20260217172854_InitialCreate	8.0.10
20260219181809_AddProcurementApplications	8.0.10
20260223151022_KeepPaymentsAndFvraApplications	8.0.10
20250917131954_SeedCpvCodes	8.0.8
20260305112006_AddApplicationGuid	8.0.10
20250922202543_AddNutsCodesTable	8.0.8
20260312115315_AddPartyRoleAndMultiRoleSupport	8.0.10
20260312145958_BackfillOrganisationsFromOrganisationInformation	8.0.10
20260312152000_BackfillUserOrganisationMembershipsFromOrganisationInformation	8.0.10
20250922203422_SeedNutsCodes	8.0.8
20251023110656_UpdateCountryCodes	8.0.8
20260313102632_AddFindATenderDefaultAccess	8.0.10
20251024155601_UpdateUkCountryCodes	8.0.8
20260313114448_BackfillEnableActiveApplicationsForOrganisations	8.0.10
20260317091038_ConsolidateRoleChanges	8.0.10
20260317150532_CorrectApplicationRoleOiScopes	8.0.10
20260324060925_BackfillFindATenderAccessFromOrganisationInformation	8.0.10
20260324064648_BackfillInviteRolesFromOrganisationInformation	8.0.10
20240625123058_CreateTenantOrganisationPerson	8.0.8
20260324134145_RemoveAgentRole	8.0.10
20240701133847_CreateForms	8.0.8
20240709175108_ConnectedEntity	8.0.8
20240711155136_AuthenticationKeys	8.0.8
20240712131609_CreateFinancialInformationSeedDate	8.0.8
20240725120036_UpdateFormSection	8.0.8
20251027165912_UpdateCpvHierarchies	8.0.8
20260218140540_RemoveCommercialToolsTables	8.0.8
20240725142711_UpdateFinancialInfoSeedData	8.0.8
20240729120131_UpdateFormAnswerSectionSet	8.0.8
20240729135311_FormAnswerGuid	8.0.8
20240729142458_RemoveStreetAddress2	8.0.8
20240729170003_AlterConnectedEntitiesAddColumnResidentCountry	8.0.8
20240730094754_FormSectionUpdate	8.0.8
20240802095312_CreateShareCodesSeedDate	8.0.8
20240806121658_FormAnswerAddressValue	8.0.8
20240806124019_AlterTypeControlCondition	8.0.8
20240807073912_MoveOrganisationToSharedConsent	8.0.8
20240809133920_AddUniqueIndexToApiKey	8.0.8
20240809155205_UpdateShareCodesSeedDate	8.0.8
20240813170602_UniqueNullableIdentifier	8.0.8
20240814081657_TweakSharedConsentEntity	8.0.8
20240819085154_RefreshToken	8.0.8
20240819160258_NonNullFormVersionInSharedConsent	8.0.8
20240820151918_FormsDataSeedRefactor	8.0.8
20240821161226_FormSectionType	8.0.8
20240822100342_CreatePersonInvite	8.0.8
20240823120456_ReplaceBookingRefWithSharerCodeInSharedConsents	8.0.8
20240823123421_CountryCode	8.0.8
20240827112509_FormAddressAnswerDataUpdate	8.0.8
20240904120425_FormNameUpdatedToStandardQuestions	8.0.8
20240911122719_AddExclusionFormSection	8.0.8
20240913144524_SharedConsentCreatedFrom	8.0.8
20240916122413_UpdateFormAnswerSet	8.0.8
20240919141551_UpdatePponSchemeName	8.0.8
20240919151457_AddNameToFormQuestion	8.0.8
20240920085144_AddApprovalFieldsToOrganisation	8.0.8
20240920113939_OutboxMessage	8.0.8
20240923101141_AlterAuthenticationKey	8.0.8
20240923162413_ExclusionFormData	8.0.8
20240925102559_BasicInformationQualifications	8.0.8
20240925151006_AlterFormSection	8.0.8
20240926155750_TradeAssuranceForm	8.0.8
20240926164114_RemoveQualificationAndTradeAssuranceTables	8.0.8
20240927091507_ChangeApprovedToReviewed	8.0.8
20240927121730_ExclusionSectionUpload	8.0.8
20240927152134_AddScopesToPerson	8.0.8
20240930111244_UpdateExclusionsEmailTitle	8.0.8
20240930215811_ExclusionFormDataGroupedSingleChoice	8.0.8
20241001080722_AddJsonValueToFormAnswer	8.0.8
20241001134038_ExclusionWebsiteQuestion	8.0.8
20241001152703_UpdateExistingIdentifiersWithUri	8.0.8
20241003082202_AddExclusionAppliesToQuestion	8.0.8
20241003102136_FixSortOrderingOfExclusionsQuestions	8.0.8
20241003131026_QuestionSortOrder	8.0.8
20241003131749_FixExclusionEnumType	8.0.8
20241007103828_ExclusionsContentTweaks	8.0.8
20241008101722_FixForUpdatingExistingIdentifiersWithUri	8.0.8
20241010090106_FormsMarkupTweaks	8.0.8
20241010135756_AddOrganisationJoinRequest	8.0.8
20241010164940_RemoveVatPrimaryIdentifiers	8.0.8
20241016134622_SupplierInformationLocalization	8.0.8
20241017142204_OrganisationPendingRoles	8.0.8
20241022113409_UpdateFormQuestionsSortOrder	8.0.8
20241023145014_PersonUserUrnIndex	8.0.8
20241028173011_UpdateAnnotationsForTrustsTrustees	8.0.8
20241029131027_FurtherQuestionsExemptedHeadings	8.0.8
20241030152146_AddDbFunctionToUpdateQuestionsSortOrder	8.0.8
20241030163144_AddExpiresOnToPersonInvite	8.0.8
20241101164409_FixForDbFunctionToUpdateQuestionsSortOrder	8.0.8
20241108162328_AnotherFixForDbFunctionToUpdateQuestionsSortOrder	8.0.8
20241112095538_QualificationsLocalization	8.0.8
20241112131018_TradeAssurancesLocalization	8.0.8
20241112152211_FinancialInformationLocalization	8.0.8
20241113115600_ExclusionsLocalization	8.0.8
20241120141822_ConsolidateExistingTenantsNamesAndIdentifiersNamesWithOrganisationsNames	8.0.8
20241204105834_AddOrganisationType	8.0.8
20241206174127_OrganisationTypeAsInteger	8.0.8
20241217224044_OutboxNotificationTrigger	8.0.8
20241218164441_OrganisationTypeEnumUpdate	8.0.8
20250103170207_AddMOUTables	8.0.8
20250106170559_OrganisationParties	8.0.8
20250107125410_ExclusionsAppliesToAnswerFieldNameConfiguration	8.0.8
20250107131438_AddMOUTables1	8.0.8
20250113160744_SeedMOU	8.0.8
20250115174623_UpdateMOUPath	8.0.8
20250120123455_OrgSearchFuzzyMatching	8.0.8
20250123085559_MoURenameBaseFile	8.0.8
20250129180119_Outbox_AddColumns	8.0.8
20250130172655_SharedConsentConsortium	8.0.8
20250131142844_ConsortiumSharedConsentFormScript	8.0.8
20250206163826_ExclusionFormQuestionEightGroupChoice	8.0.8
20250210131934_FixConsortiumMigration	8.0.8
20250210154508_FixConsortiumMigration2	8.0.8
20250213174807_Add_Missing_Form_Translations	8.0.8
20250220114020_OrgNameCaseInsensitiveIndex	8.0.8
20250220152828_PostalCodeNuallable	8.0.8
20250225102501_IdentifierSchemaUniqueIndex	8.0.8
20250226180556_GetSharedConsentDetailsSp	8.0.8
20250227163733_PersonsAddPreviousUrnsColumn	8.0.8
20250228145709_PersonsCaseInsensitiveEmailIndex	8.0.8
20250313174211_SharedConsentDetailsProcUpdate	8.0.8
20250315000609_SharedConsentSnapshot	8.0.8
20250318085620_SharedConsentRemoveConnectedPersonsEndDateLogic	8.0.8
20250319102443_PponIdentifierUrlFix	8.0.8
20250328110118_FixConnectedEntityMigration	8.0.8
20250328121109_AddDescriptionHintFinancialInformation03	8.0.8
20250401163725_MouEmailReminder	8.0.8
20250402144802_Add_Connected_Entities_Deleted_Column	8.0.8
20250410111341_UpdateFormQuestionDescTitleCaption	8.0.8
20250416164328_AdditionalConnectedEntityInfo	8.0.8
20250425105930_AddAnnouncements	8.0.8
20250428110309_UpdateFormSectionsConfigurationforAnswerSummary	8.0.8
20250428144556_ExclusionEightGroupChoiceQuestionUpdate	8.0.8
20250429102036_MovingToGitManagedStoredProcedures	8.0.8
20250501075954_MovingDataSharingApiFixesToGitManagedStoredProcedures	8.0.8
20250502080309_MovingConnectedEntitiesFixesToGitManagedStoredProcedures	8.0.8
20250502120159_ReloadStoredProceduresAfterGitMerge	8.0.8
20250516213618_SteelSectionForm	8.0.8
20250519143446_CarbonNetZeroSectionForm	8.0.8
20250522143447_HealthAndSafetySectionForm	8.0.8
20250602124055_UpdateSteelSectionForm	8.0.8
20250604083824_AddConsortiumRole	8.0.8
20250604113701_ModernSlaverySectionForm	8.0.8
20250611082829_CyberEssentialsSectionForm	8.0.8
20250617181527_RemoveCarbonNetZeroSectionAndQuestions	8.0.8
20250619120330_DataProtectionSectionForm	8.0.8
20250626154729_AddOrganisationHierarchiesTable	8.0.8
20250710141010_PaymentsSectionForm	8.0.8
20250729130100_ReapplyCarbonNetZeroFormUpdate	8.0.8
20250729165422_UpdateDataForm	8.0.8
20250729224843_RemoveMultipleAnswerSetsFromASM	8.0.8
20250730093016_UpdateCyberEssentialsForm	8.0.8
20250731154500_GroupPaymentsQuestions	8.0.8
20250804003113_FixCarbonNetZeroQuestionsFlow	8.0.8
20250804152420_RefactorLayoutOptions	8.0.8
20250804170301_UpdateCaptionAndStartButtons	8.0.8
20250804181147_OrderAsmAlphabetically	8.0.8
20250804183208_AddFutureDateValidation	8.0.8
20250829112726_WelshExclusionsForm	8.0.8
20250904141716_QualityManagementSectionForm	8.0.8
20250911123145_UpdateQualityManagementSectionForm	8.0.8
20250912110337_WelshSteelSectionForm	8.0.8
20250915145138_EnvironmentalSectionForm	8.0.8
20250925133856_WelshHealthAndSafetySectionForm	8.0.8
20251112135347_UpdatePaymentsSectionForm	8.0.8
20251127151709_AddWelshHealthAndSafetyQuestion02Description	8.0.8
20251211115513_UpdateWelshHealthAndSafetyQuestions	8.0.8
20251211154954_UpdateWelshExclusions	8.0.8
20251216091815_UpdateWelshSteelSectionForm	8.0.8
20251216113944_Update_2_QualityManagement	8.0.8
20251217102327_UpdateWelshEnvironmentalQuestions	8.0.8
20260120112501_UpdateModernSlaveryQuestions	8.0.8
20260128160628_UpdatePaymentsQuestions	8.0.8
20260202111918_UpdateHealthAndSafety_03	8.0.8
20260205151018_UpdateCarbonNetZeroQuestion	8.0.8
20260219171520_GrantReadOrganisationDataScopeToServiceKeys	8.0.8
20260320112027_UpdateLatestServiceKeyScopes	8.0.8
20260323183057_SeedUserManagementServiceKey	8.0.8
\.


--
-- Data for Name: addresses; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.addresses (id, street_address, locality, region, postal_code, country_name, created_on, updated_on, country) FROM stdin;
1	123 Test Street	London	\N	DA11 8HJ	United Kingdom	2026-04-15 12:24:24.671724+00	2026-04-15 12:24:24.671724+00	GB
2	41599 Lupe Hill	North Kaleb		sj34 5sb	Northern Ireland	2026-04-15 12:27:18.284106+00	2026-04-15 12:27:18.284106+00	GB
3	923 Dietrich Radial	New Retha		cz81 7sd	Scotland	2026-04-15 12:42:04.318545+00	2026-04-15 12:42:04.318545+00	GB
4	89958 Mafalda Pike	Terranceborough		fl2 8hr	Scotland	2026-04-15 12:53:39.965838+00	2026-04-15 12:53:39.965838+00	GB
5	3050 McClure Course	New Sidmouth		du19 3gv	England	2026-04-15 12:59:15.237882+00	2026-04-15 12:59:15.237882+00	GB
6	936 Ivy Stravenue	Parisianhaven		zx5 3ed	Wales	2026-04-15 13:04:51.790952+00	2026-04-15 13:04:51.790952+00	GB
7	6930 Stan Burgs	Rathborough		yh7 1wi	Northern Ireland	2026-04-15 13:08:19.018298+00	2026-04-15 13:08:19.018298+00	GB
8	5440 Wyman Lakes	Zulauffort		af4 1ns	Wales	2026-04-15 13:13:04.88707+00	2026-04-15 13:13:04.88707+00	GB
9	91956 Douglas Drives	South Juanitaborough		yb28 1kt	England	2026-04-15 13:15:30.06984+00	2026-04-15 13:15:30.06984+00	GB
10	0411 Corbin Glens	Mitchellmouth		il1 6cb	England	2026-04-15 13:18:56.009106+00	2026-04-15 13:18:56.009106+00	GB
11	668 Moriah Springs	Jaskolskifort		rx3 3gc	Wales	2026-04-15 13:26:54.138169+00	2026-04-15 13:26:54.138169+00	GB
12	3779 Bartell Coves	New Burleytown		kn05 9kf	England	2026-04-15 13:30:00.970274+00	2026-04-15 13:30:00.970274+00	GB
13	08833 Mylene Walk	Charleneton		ci6 1vb	Scotland	2026-04-15 13:32:11.253718+00	2026-04-15 13:32:11.253718+00	GB
14	7680 Orland Ford	Lake Gerardtown		fp56 6tl	Northern Ireland	2026-04-15 13:35:53.988093+00	2026-04-15 13:35:53.988093+00	GB
15	9154 Oberbrunner Rapids	Waelchitown		wl2 1ch	Northern Ireland	2026-04-15 13:37:00.803533+00	2026-04-15 13:37:00.803533+00	GB
16	497 Herman Drives	Prosaccoland		yf66 4pp	Northern Ireland	2026-04-15 13:38:24.126018+00	2026-04-15 13:38:24.126018+00	GB
17	90604 Botsford Ways	Kleinland		dk3 7nc	Northern Ireland	2026-04-15 13:42:39.025353+00	2026-04-15 13:42:39.025353+00	GB
18	5634 Colleen Plain	Wardburgh		za55 2pb	Scotland	2026-04-15 13:44:41.690217+00	2026-04-15 13:44:41.690217+00	GB
19	8275 Langosh Crossroad	Lake Haylee		tv7 5fx	Northern Ireland	2026-04-15 13:45:44.042358+00	2026-04-15 13:45:44.042358+00	GB
20	23519 Wisoky Junction	North Vaughnburgh		zo7 0oq	Wales	2026-04-15 13:48:07.772765+00	2026-04-15 13:48:07.772765+00	GB
\.


--
-- Data for Name: addresses_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.addresses_snapshot (id, street_address, locality, region, postal_code, country_name, country, created_on, updated_on, mapping_id) FROM stdin;
\.


--
-- Data for Name: announcements; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.announcements (id, guid, title, body, url_regex, start_date, end_date, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: persons; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.persons (id, guid, first_name, last_name, email, phone, user_urn, created_on, updated_on, scopes, previous_urns) FROM stdin;
1	72dadc5a-e227-4ff6-ba32-406051f5ff0c	Kieren	Woods	kieren.woods@goaco.com	\N	urn:fdc:gov.uk:2022:CmmQZV9jeFEPD9bDVc6uyUgwG8O_cnv_8zHBu_sUQo4	2026-04-15 08:29:52.427323+00	2026-04-15 08:29:52.427323+00	{SUPERADMIN,FTSSUPPORT}	{}
3	f7b39c24-f2a3-4b03-9b80-7c86eb2ac9bf	Kieren	Woods	kierenlwoods@gmail.com	\N	urn:fdc:gov.uk:2022:PHpT9f6npgnuMN6fRPIpdUyw90Gz0JBnJIwomf4Bkjs	2026-04-15 13:11:08.728432+00	2026-04-15 13:11:08.728432+00	{}	{}
\.


--
-- Data for Name: tenants; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.tenants (id, guid, name, created_on, updated_on) FROM stdin;
1	3b04f934-2de9-433c-8c7c-93ce77278c41	ConnectedPersonsOrg 220C02F7ECD045	2026-04-15 12:24:24.671724+00	2026-04-15 12:24:24.671724+00
2	8795c384-2c7b-4135-9a96-d1102e653a87	Heller LLC	2026-04-15 12:27:18.284106+00	2026-04-15 12:27:18.284106+00
3	0b332945-0982-4617-b512-836e82f144cb	Pfeffer-Kohler	2026-04-15 12:42:04.318545+00	2026-04-15 12:42:04.318545+00
4	3996d152-3632-42ea-ae5c-17fca002d40d	Zieme LLC	2026-04-15 12:53:39.965838+00	2026-04-15 12:53:39.965838+00
5	d634a68f-d70d-45a5-87b1-ef5ddbb73210	Schoen, Homenick and McLaughlin	2026-04-15 12:59:15.237882+00	2026-04-15 12:59:15.237882+00
6	6735afc5-eab5-43c3-ac00-f0efdbe5faf5	White-Rodriguez	2026-04-15 13:04:51.790952+00	2026-04-15 13:04:51.790952+00
7	5c89d899-b4dd-419b-9f4a-6cf1254bd03b	Block-Green	2026-04-15 13:08:19.018298+00	2026-04-15 13:08:19.018298+00
8	aaceea64-6150-4f8b-8e65-20f3c0e7b890	Pollich, Homenick and Carroll	2026-04-15 13:13:04.88707+00	2026-04-15 13:13:04.88707+00
9	9493b6eb-5a96-499d-b962-9c9d50386d18	Lesch-Treutel	2026-04-15 13:15:30.06984+00	2026-04-15 13:15:30.06984+00
10	9c4a3741-4a89-47ba-ac9c-045e6a1eec24	Rohan, Treutel and Emard	2026-04-15 13:18:56.009106+00	2026-04-15 13:18:56.009106+00
11	3d510481-64ad-4baf-ae3f-58760637853b	Powlowski-Rodriguez	2026-04-15 13:26:54.138169+00	2026-04-15 13:26:54.138169+00
12	2860db20-3d0d-4466-9347-dc64c6fff579	Schuster-Little	2026-04-15 13:30:00.970274+00	2026-04-15 13:30:00.970274+00
13	5b8a3f68-8a62-4e24-8bb8-816f4ade569b	Stoltenberg, Wintheiser and Bartell	2026-04-15 13:32:11.253718+00	2026-04-15 13:32:11.253718+00
14	ff1f3039-f220-4d19-80eb-c1ca916c682d	Davis LLC	2026-04-15 13:35:53.988093+00	2026-04-15 13:35:53.988093+00
15	386481d2-8c2b-473e-87a9-6d55d0ead0be	Lindgren, Will and Stroman	2026-04-15 13:37:00.803533+00	2026-04-15 13:37:00.803533+00
16	72263301-3033-4e33-8d5d-8bafc3e2fc81	Kohler Inc and Sons	2026-04-15 13:38:24.126018+00	2026-04-15 13:38:24.126018+00
17	b417c020-e371-424e-b4b1-79e8514a0e46	Marks Group	2026-04-15 13:42:39.025353+00	2026-04-15 13:42:39.025353+00
18	c42e1091-132c-4fd2-aba1-275f46262e8a	Waters LLC	2026-04-15 13:44:41.690217+00	2026-04-15 13:44:41.690217+00
19	9ed5cdeb-36da-4059-b27f-a83aa743cf07	Stoltenberg Group	2026-04-15 13:45:44.042358+00	2026-04-15 13:45:44.042358+00
20	04a89ccf-d08e-47a5-bf90-b325dac59f80	Raynor-Gleichner	2026-04-15 13:48:07.772765+00	2026-04-15 13:48:07.772765+00
\.


--
-- Data for Name: organisations; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.organisations (id, guid, tenant_id, name, roles, created_on, updated_on, reviewed_by_id, approved_on, review_comment, pending_roles, type) FROM stdin;
1	13f6ff41-6ce3-4b7e-908d-d809a655cf6e	1	ConnectedPersonsOrg 220C02F7ECD045	{}	2026-04-15 12:24:24.671724+00	2026-04-15 12:24:24.671724+00	\N	\N	\N	{1}	1
17	6ee2465b-4664-41cd-a914-d53fe204af5e	17	Marks Group	{1}	2026-04-15 13:42:39.025353+00	2026-04-15 13:42:39.32955+00	1	2026-04-15 13:42:39.324135+00	Approved via API	{}	1
2	4344ef5e-ddcf-41d2-b5c7-4a5ca13df128	2	Heller LLC	{1}	2026-04-15 12:27:18.284106+00	2026-04-15 12:27:18.498592+00	1	2026-04-15 12:27:18.49574+00	Approved via API	{}	1
3	829b0d39-9db3-4325-b506-81c325dc8936	3	Pfeffer-Kohler	{1}	2026-04-15 12:42:04.318545+00	2026-04-15 12:42:04.586365+00	1	2026-04-15 12:42:04.585481+00	Approved via API	{}	1
4	d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5	4	Zieme LLC	{1}	2026-04-15 12:53:39.965838+00	2026-04-15 12:53:40.222384+00	1	2026-04-15 12:53:40.221304+00	Approved via API	{}	1
18	54ecfe58-b278-46cc-9c6f-c62295ef5505	18	Waters LLC	{1}	2026-04-15 13:44:41.690217+00	2026-04-15 13:44:42.002057+00	1	2026-04-15 13:44:42.001829+00	Approved via API	{}	1
5	e73a5c46-b8e6-46f9-8fb9-59af6800f768	5	Schoen, Homenick and McLaughlin	{1}	2026-04-15 12:59:15.237882+00	2026-04-15 12:59:15.483673+00	1	2026-04-15 12:59:15.483419+00	Approved via API	{}	1
6	a849f727-140e-4c6e-b4cb-7fdcbc2506d1	6	White-Rodriguez	{1}	2026-04-15 13:04:51.790952+00	2026-04-15 13:04:51.996497+00	1	2026-04-15 13:04:51.996126+00	Approved via API	{}	1
7	2f844e27-18e3-42e7-a136-221e91c75753	7	Block-Green	{1}	2026-04-15 13:08:19.018298+00	2026-04-15 13:08:19.247275+00	1	2026-04-15 13:08:19.246686+00	Approved via API	{}	1
19	d35f709c-9af4-4a25-815e-a01653bc5de3	19	Stoltenberg Group	{1}	2026-04-15 13:45:44.042358+00	2026-04-15 13:45:44.167928+00	1	2026-04-15 13:45:44.167511+00	Approved via API	{}	1
8	57898244-28be-4dc8-a6a6-ddfb96052861	8	Pollich, Homenick and Carroll	{1}	2026-04-15 13:13:04.88707+00	2026-04-15 13:13:05.093609+00	1	2026-04-15 13:13:05.093392+00	Approved via API	{}	1
9	9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a	9	Lesch-Treutel	{1}	2026-04-15 13:15:30.06984+00	2026-04-15 13:15:30.291551+00	1	2026-04-15 13:15:30.291375+00	Approved via API	{}	1
10	0db96436-e4f0-4bcc-8825-b828910bf0b3	10	Rohan, Treutel and Emard	{1}	2026-04-15 13:18:56.009106+00	2026-04-15 13:18:56.326331+00	1	2026-04-15 13:18:56.326107+00	Approved via API	{}	1
20	19c08015-d446-45b4-92bf-5bbc45c06bce	20	Raynor-Gleichner	{1}	2026-04-15 13:48:07.772765+00	2026-04-15 13:48:07.972815+00	1	2026-04-15 13:48:07.972634+00	Approved via API	{}	1
11	2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98	11	Powlowski-Rodriguez	{1}	2026-04-15 13:26:54.138169+00	2026-04-15 13:26:54.491379+00	1	2026-04-15 13:26:54.485865+00	Approved via API	{}	1
12	d37589cf-2ded-4c9d-b956-a237de7d8b36	12	Schuster-Little	{1}	2026-04-15 13:30:00.970274+00	2026-04-15 13:30:01.151109+00	1	2026-04-15 13:30:01.150539+00	Approved via API	{}	1
13	0032ec59-3a20-4854-a4ed-3681f1b5c79a	13	Stoltenberg, Wintheiser and Bartell	{1}	2026-04-15 13:32:11.253718+00	2026-04-15 13:32:11.461296+00	1	2026-04-15 13:32:11.460923+00	Approved via API	{}	1
14	9502bf6a-3fca-4a7b-8a51-f74aca96e2db	14	Davis LLC	{1}	2026-04-15 13:35:53.988093+00	2026-04-15 13:35:54.203027+00	1	2026-04-15 13:35:54.202669+00	Approved via API	{}	1
15	e8429bd6-05ee-4872-ba6f-a5539e6e0a3b	15	Lindgren, Will and Stroman	{1}	2026-04-15 13:37:00.803533+00	2026-04-15 13:37:00.934572+00	1	2026-04-15 13:37:00.934355+00	Approved via API	{}	1
16	587cdb5b-1ca3-4fc2-9e03-e740f95b37a5	16	Kohler Inc and Sons	{1}	2026-04-15 13:38:24.126018+00	2026-04-15 13:38:24.463104+00	1	2026-04-15 13:38:24.462685+00	Approved via API	{}	1
\.


--
-- Data for Name: authentication_keys; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.authentication_keys (id, name, key, organisation_id, scopes, created_on, updated_on, revoked, revoked_on) FROM stdin;
1	Heller LLC	Heller LLC	2	[]	2026-04-15 12:27:18.621773+00	2026-04-15 12:27:18.621773+00	f	\N
2	Pfeffer-Kohler	Pfeffer-Kohler	3	[]	2026-04-15 12:42:36.326072+00	2026-04-15 12:42:36.326072+00	f	\N
3	Zieme LLC	Zieme LLC	4	[]	2026-04-15 12:54:47.391465+00	2026-04-15 12:54:47.391465+00	f	\N
4	Schoen, Homenick and McLaughlin	Schoen, Homenick and McLaughlin	5	[]	2026-04-15 12:59:50.883867+00	2026-04-15 12:59:50.883867+00	f	\N
5	White-Rodriguez	White-Rodriguez	6	[]	2026-04-15 13:04:52.143856+00	2026-04-15 13:04:52.143856+00	f	\N
6	Block-Green	Block-Green	7	[]	2026-04-15 13:08:19.346002+00	2026-04-15 13:08:19.346002+00	f	\N
7	Pollich, Homenick and Carroll	Pollich, Homenick and Carroll	8	[]	2026-04-15 13:13:05.197042+00	2026-04-15 13:13:05.197042+00	f	\N
8	Lesch-Treutel	Lesch-Treutel	9	[]	2026-04-15 13:15:30.405732+00	2026-04-15 13:15:30.405732+00	f	\N
9	Rohan, Treutel and Emard	Rohan, Treutel and Emard	10	[]	2026-04-15 13:18:56.420215+00	2026-04-15 13:18:56.420215+00	f	\N
10	FTS_SRSI_API_KEY	key-123	\N	["read:organisation_data"]	2026-04-15 13:25:12.934202+00	2026-04-15 13:25:12.934202+00	f	\N
11	Powlowski-Rodriguez	Powlowski-Rodriguez	11	[]	2026-04-15 13:26:54.628667+00	2026-04-15 13:26:54.628667+00	f	\N
12	Schuster-Little	Schuster-Little	12	[]	2026-04-15 13:30:01.25436+00	2026-04-15 13:30:01.25436+00	f	\N
13	Stoltenberg, Wintheiser and Bartell	Stoltenberg, Wintheiser and Bartell	13	[]	2026-04-15 13:32:11.560095+00	2026-04-15 13:32:11.560095+00	f	\N
14	Davis LLC	Davis LLC	14	[]	2026-04-15 13:35:54.342448+00	2026-04-15 13:35:54.342448+00	f	\N
15	Kohler Inc and Sons	Kohler Inc and Sons	16	[]	2026-04-15 13:38:24.562778+00	2026-04-15 13:38:24.562778+00	f	\N
16	Marks Group	Marks Group	17	[]	2026-04-15 13:42:39.436885+00	2026-04-15 13:42:39.436885+00	f	\N
17	Waters LLC	Waters LLC	18	[]	2026-04-15 13:44:42.166308+00	2026-04-15 13:44:42.166308+00	f	\N
18	Stoltenberg Group	Stoltenberg Group	19	[]	2026-04-15 13:45:44.234706+00	2026-04-15 13:45:44.234706+00	f	\N
19	Raynor-Gleichner	Raynor-Gleichner	20	[]	2026-04-15 13:48:08.100857+00	2026-04-15 13:48:08.100857+00	f	\N
\.


--
-- Data for Name: buyer_information; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.buyer_information (id, buyer_type, devolved_regulations, created_on, updated_on) FROM stdin;
1	\N	{}	2026-04-15 12:24:24.671724+00	2026-04-15 12:24:24.671724+00
2	PrivateUtility	{}	2026-04-15 12:27:18.284106+00	2026-04-15 12:27:18.453755+00
3	PrivateUtility	{}	2026-04-15 12:42:04.318545+00	2026-04-15 12:42:04.569484+00
4	PrivateUtility	{}	2026-04-15 12:53:39.965838+00	2026-04-15 12:53:40.204624+00
5	PrivateUtility	{}	2026-04-15 12:59:15.237882+00	2026-04-15 12:59:15.467326+00
6	PrivateUtility	{}	2026-04-15 13:04:51.790952+00	2026-04-15 13:04:51.984879+00
7	PrivateUtility	{}	2026-04-15 13:08:19.018298+00	2026-04-15 13:08:19.237683+00
8	PrivateUtility	{}	2026-04-15 13:13:04.88707+00	2026-04-15 13:13:05.083987+00
9	PrivateUtility	{}	2026-04-15 13:15:30.06984+00	2026-04-15 13:15:30.280648+00
10	PrivateUtility	{}	2026-04-15 13:18:56.009106+00	2026-04-15 13:18:56.315102+00
11	PrivateUtility	{}	2026-04-15 13:26:54.138169+00	2026-04-15 13:26:54.461895+00
12	PrivateUtility	{}	2026-04-15 13:30:00.970274+00	2026-04-15 13:30:01.140891+00
13	PrivateUtility	{}	2026-04-15 13:32:11.253718+00	2026-04-15 13:32:11.450575+00
14	PrivateUtility	{}	2026-04-15 13:35:53.988093+00	2026-04-15 13:35:54.184794+00
15	PrivateUtility	{}	2026-04-15 13:37:00.803533+00	2026-04-15 13:37:00.912709+00
16	PrivateUtility	{}	2026-04-15 13:38:24.126018+00	2026-04-15 13:38:24.44845+00
17	PrivateUtility	{}	2026-04-15 13:42:39.025353+00	2026-04-15 13:42:39.284767+00
18	PrivateUtility	{}	2026-04-15 13:44:41.690217+00	2026-04-15 13:44:41.989261+00
19	PrivateUtility	{}	2026-04-15 13:45:44.042358+00	2026-04-15 13:45:44.157355+00
20	PrivateUtility	{}	2026-04-15 13:48:07.772765+00	2026-04-15 13:48:07.964627+00
\.


--
-- Data for Name: connected_entities; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.connected_entities (id, guid, entity_type, has_company_house_number, company_house_number, overseas_company_number, registered_date, register_name, supplier_organisation_id, end_date, created_on, updated_on, deleted) FROM stdin;
\.


--
-- Data for Name: forms; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.forms (id, guid, name, version, is_required, scope, created_on, updated_on) FROM stdin;
4	0618b13e-eaf2-46e3-a7d2-6f2c44be7022	Standard Questions	1.0	t	0	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00
6	24482a2a-88a8-4432-b03c-4c966c9fce23	Consortium Information Form	1.0	t	0	2026-04-15 08:25:29.556123+00	2026-04-15 08:25:29.556123+00
\.


--
-- Data for Name: shared_consents; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.shared_consents (id, guid, organisation_id, form_id, submission_state, submitted_at, form_version_id, share_code, created_on, updated_on, created_from) FROM stdin;
\.


--
-- Data for Name: connected_entities_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.connected_entities_snapshot (id, shared_consent_id, guid, entity_type, has_company_house_number, company_house_number, overseas_company_number, registered_date, register_name, end_date, created_on, updated_on, mapping_id, deleted) FROM stdin;
\.


--
-- Data for Name: connected_entity_address; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.connected_entity_address (id, connected_entity_id, type, address_id) FROM stdin;
\.


--
-- Data for Name: connected_entity_address_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.connected_entity_address_snapshot (id, type, address_id, connected_entity_snapshot_id) FROM stdin;
\.


--
-- Data for Name: connected_individual_trust; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.connected_individual_trust (connected_individual_trust_id, category, first_name, last_name, date_of_birth, nationality, control_condition, connected_type, person_id, created_on, updated_on, resident_country) FROM stdin;
\.


--
-- Data for Name: connected_individual_trust_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.connected_individual_trust_snapshot (id, category, first_name, last_name, date_of_birth, nationality, control_condition, connected_type, person_id, resident_country, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: connected_organisation; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.connected_organisation (connected_organisation_id, category, name, insolvency_date, registered_legal_form, law_registered, control_condition, organisation_id, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: connected_organisation_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.connected_organisation_snapshot (id, category, name, insolvency_date, registered_legal_form, law_registered, control_condition, organisation_id, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: contact_points; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.contact_points (id, name, email, telephone, url, created_on, updated_on, organisation_id) FROM stdin;
1	Test Contact	contact@test.com	079256123321	https://test.com	2026-04-15 12:24:24.671724+00	2026-04-15 12:24:24.671724+00	1
2	Daron Hahn	kailee.ohara@smitham.info	856.723.9196	http://www.millerwintheiser.name/shop/food/page.jsp	2026-04-15 12:27:18.284106+00	2026-04-15 12:27:18.284106+00	2
3	Mrs. Evelyn Bell Lebsack PhD	trinity@gottlieb.name	(819)571-0223	http://www.watsica.ca/interviews/form.gem	2026-04-15 12:42:04.318545+00	2026-04-15 12:42:04.318545+00	3
4	Shayna Mitchell	clovis_dickinson@douglas.biz	1-650-453-6197 x96149	http://www.williamson.com/catalog/form.gem	2026-04-15 12:53:39.965838+00	2026-04-15 12:53:39.965838+00	4
5	Maiya McDermott	dayna.shanahan@barrows.co.uk	359.528.9669	http://www.ernser.biz/reviews/applet.rsx	2026-04-15 12:59:15.237882+00	2026-04-15 12:59:15.237882+00	5
6	Dewayne Volkman	celia@bauch.com	(294)232-2264 x17204	http://www.stark.info/interviews/page.asp	2026-04-15 13:04:51.790952+00	2026-04-15 13:04:51.790952+00	6
7	Hardy Rippin	curt_ohara@wilderman.co.uk	199-854-2670 x2596	http://www.quigley.ca/shop/films/template.aspx	2026-04-15 13:08:19.018298+00	2026-04-15 13:08:19.018298+00	7
8	Mr. Ed Kane Schumm Sr.	soledad_goyette@spinkabraun.us	025.685.8535	http://www.yost.biz/home/template.html	2026-04-15 13:13:04.88707+00	2026-04-15 13:13:04.88707+00	8
9	Christy Terence Fahey III	clarissa@turcottestehr.ca	1-883-508-5531 x417	http://www.windlerorn.uk/home/form.htm	2026-04-15 13:15:30.06984+00	2026-04-15 13:15:30.06984+00	9
10	Johathan Barrows	ana@reichel.biz	(580)473-3474 x45079	http://www.trantow.info/shop/food/root.res	2026-04-15 13:18:56.009106+00	2026-04-15 13:18:56.009106+00	10
11	Mr. Tanya Carey Mraz Jr.	halle@haneromaguera.info	(503)906-0657 x17127	http://www.mitchellhoeger.co.uk/interviews/template.htm	2026-04-15 13:26:54.138169+00	2026-04-15 13:26:54.138169+00	11
12	Haleigh Williamson	orion@jakubowski.us	065.048.9397	http://www.armstrongoberbrunner.ca/guide/index.asp	2026-04-15 13:30:00.970274+00	2026-04-15 13:30:00.970274+00	12
13	Donald Homenick	isabella.cole@mitchell.name	496-953-4666 x1430	http://www.collier.ca/home/template.jsp	2026-04-15 13:32:11.253718+00	2026-04-15 13:32:11.253718+00	13
14	Hellen Beatty	marques@connelly.com	(220)802-4168	http://www.dickinson.ca/shop/films/resource.html	2026-04-15 13:35:53.988093+00	2026-04-15 13:35:53.988093+00	14
15	Freddy Rogahn Sr.	ali_maggio@morissetteokuneva.uk	264.040.3318 x667	http://www.beier.com/home/page.htm	2026-04-15 13:37:00.803533+00	2026-04-15 13:37:00.803533+00	15
16	Webster Schoen	guillermo.klocko@harvey.uk	(278)595-9712	http://www.flatley.ca/films/template.htm	2026-04-15 13:38:24.126018+00	2026-04-15 13:38:24.126018+00	16
17	Ezequiel Devante Leffler Jr.	larue_ziemann@legros.co.uk	256-458-3481	http://www.mclaughlin.name/shop/food/page.rsx	2026-04-15 13:42:39.025353+00	2026-04-15 13:42:39.025353+00	17
18	Ramona Keeling	armani@rowe.uk	284-236-5083 x90474	http://www.frami.com/shop/books/root.rsx	2026-04-15 13:44:41.690217+00	2026-04-15 13:44:41.690217+00	18
19	Kraig Wanda Weimann DDS	curt.dach@tremblay.name	1-170-022-4412 x37573	http://www.volkman.com/articles/template.aspx	2026-04-15 13:45:44.042358+00	2026-04-15 13:45:44.042358+00	19
20	Garnet Miller	velma@harber.us	929-261-5660	http://www.hills.uk/reviews/form.html	2026-04-15 13:48:07.772765+00	2026-04-15 13:48:07.772765+00	20
\.


--
-- Data for Name: contact_points_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.contact_points_snapshot (id, shared_consent_id, name, email, telephone, url, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: form_sections; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.form_sections (id, guid, title, form_id, created_on, updated_on, configuration, allows_multiple_answer_sets, type, check_further_questions_exempted, display_order) FROM stdin;
5	936096b3-c3bb-4475-ad7d-73b44ff61e76	ShareMyInformation_SectionTitle	4	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	{}	f	1	f	100
9	463998ef-faac-400c-b5f2-e7b24997d1a3	Share_My_Information_Consortium_SectionTitle	6	2026-04-15 08:25:29.556123+00	2026-04-15 08:25:29.556123+00	{}	f	1	f	101
6	8a75cb04-fe29-45ae-90f9-168832dbea48	Exclusions_SectionTitle	4	2026-04-15 08:25:27.416983+00	2026-04-15 08:25:27.416983+00	{"AddAnotherAnswerLabel": "Exclusions_Configuration_AddAnotherAnswerLabel", "SingularSummaryHeading": "Exclusions_Configuration_SingularSummaryHeading", "SummaryRenderFormatter": {"KeyParams": ["_Exclusion09"], "ValueParams": ["_Exclusion08"], "KeyExpression": "{0}", "ValueExpression": "{0}", "KeyExpressionOperation": "StringFormat", "ValueExpressionOperation": "StringFormat"}, "RemoveConfirmationCaption": "Exclusions_SectionTitle", "RemoveConfirmationHeading": "Exclusions_Configuration_RemoveConfirmationHeading", "PluralSummaryHeadingFormat": "Exclusions_Configuration_PluralSummaryHeadingFormat", "SingularSummaryHeadingHint": "Exclusions_Configuration_SingularSummaryHeadingHint", "PluralSummaryHeadingHintFormat": "Exclusions_Configuration_PluralSummaryHeadingHintFormat", "FurtherQuestionsExemptedHeading": "Exclusions_Configuration_FurtherQuestionsExemptedHeading"}	t	2	t	3
7	798cf1c1-40be-4e49-9adb-252219d5599d	Qualifications_SectionTitle	4	2026-04-15 08:25:27.71836+00	2026-04-15 08:25:27.71836+00	{"AddAnotherAnswerLabel": "Qualifications_Configuration_AddAnotherAnswerLabel", "SingularSummaryHeading": "Qualifications_Configuration_SingularSummaryHeading", "SummaryRenderFormatter": {"KeyParams": ["_Qualifications01"], "ValueParams": ["_Qualifications03"], "KeyExpression": "{0}", "ValueExpression": "Qualifications_02_SummaryValue", "KeyExpressionOperation": "StringFormat", "ValueExpressionOperation": "StringFormat"}, "RemoveConfirmationCaption": "Qualifications_SectionTitle", "RemoveConfirmationHeading": "Qualifications_Configuration_RemoveConfirmationHeading", "PluralSummaryHeadingFormat": "Qualifications_Configuration_PluralSummaryHeadingFormat", "SingularSummaryHeadingHint": "Qualifications_Configuration_SingularSummaryHeadingHint", "FurtherQuestionsExemptedHint": "Qualifications_Configuration_FurtherQuestionsExemptedHint", "PluralSummaryHeadingHintFormat": "Qualifications_Configuration_PluralSummaryHeadingHintFormat", "FurtherQuestionsExemptedHeading": "Qualifications_Configuration_FurtherQuestionsExemptedHeading"}	t	0	t	1
8	cf08acf8-e2fa-40c8-83e7-50c8671c343f	TradeAssurances_SectionTitle	4	2026-04-15 08:25:27.777286+00	2026-04-15 08:25:27.777286+00	{"AddAnotherAnswerLabel": "TradeAssurances_Configuration_AddAnotherAnswerLabel", "SingularSummaryHeading": "TradeAssurances_Configuration_SingularSummaryHeading", "SummaryRenderFormatter": {"KeyParams": ["_TradeAssurance01"], "ValueParams": ["_TradeAssurance03"], "KeyExpression": "{0}", "ValueExpression": "TradeAssurance_01_SummaryValue", "KeyExpressionOperation": "StringFormat", "ValueExpressionOperation": "StringFormat"}, "RemoveConfirmationCaption": "TradeAssurances_SectionTitle", "RemoveConfirmationHeading": "TradeAssurances_Configuration_RemoveConfirmationHeading", "PluralSummaryHeadingFormat": "TradeAssurances_Configuration_PluralSummaryHeadingFormat", "SingularSummaryHeadingHint": "TradeAssurances_Configuration_SingularSummaryHeadingHint", "FurtherQuestionsExemptedHint": "TradeAssurances_Configuration_FurtherQuestionsExemptedHint", "PluralSummaryHeadingHintFormat": "TradeAssurances_Configuration_PluralSummaryHeadingHintFormat", "FurtherQuestionsExemptedHeading": "TradeAssurances_Configuration_FurtherQuestionsExemptedHeading"}	t	0	t	2
4	13511cb1-9ed4-4d72-ba9e-05b4a0be880c	FinancialInformation_SectionTitle	4	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	{"AddAnotherAnswerLabel": "FinancialInformation_Configuration_AddAnotherAnswerLabel", "SingularSummaryHeading": "FinancialInformation_Configuration_SingularSummaryHeading", "SummaryRenderFormatter": {"KeyParams": ["_FinancialInformation03", "False"], "ValueParams": ["_FinancialInformation01"], "KeyExpression": "FinancialInformation_SummaryList_Key", "ValueExpression": "FinancialInformation_SummaryList_Value", "KeyExpressionOperation": "Equality", "ValueExpressionOperation": "StringFormat"}, "RemoveConfirmationCaption": "FinancialInformation_SectionTitle", "RemoveConfirmationHeading": "FinancialInformation_Configuration_RemoveConfirmationHeading", "PluralSummaryHeadingFormat": "FinancialInformation_Configuration_PluralSummaryHeadingFormat", "SingularSummaryHeadingHint": "FinancialInformation_Configuration_SingularSummaryHeadingHint", "FurtherQuestionsExemptedHint": "FinancialInformation_Configuration_FurtherQuestionsExemptedHint", "PluralSummaryHeadingHintFormat": "FinancialInformation_Configuration_PluralSummaryHeadingHintFormat", "FurtherQuestionsExemptedHeading": "FinancialInformation_Configuration_FurtherQuestionsExemptedHeading"}	t	0	f	4
10	5f1856f7-8863-4a0f-8b6d-011fa5f54d9c	Steel_SectionTitle	4	2026-04-15 08:25:30.902389+00	2026-04-15 08:25:30.902389+00	{}	f	3	f	7
12	0fbc1f86-d990-4ecc-9bda-ffc47b663cf9	HealthAndSafety_SectionTitle	4	2026-04-15 08:25:30.978512+00	2026-04-15 08:25:30.978512+00	{}	f	3	f	4
13	a13b4b12-7090-458c-93bf-126ff905bde1	ModernSlavery_SectionTitle	4	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	{}	f	3	f	5
14	3df850db-5896-4598-bc42-2c442dfc71ab	CyberEssentials_SectionTitle	4	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	{}	f	3	f	2
15	360794f9-c204-4c1c-8c47-b0336e664fff	DataProtection_SectionTitle	4	2026-04-15 08:25:31.226056+00	2026-04-15 08:25:31.226056+00	{}	f	3	f	3
16	fadd6253-8166-4952-aae6-e082860542b9	Payments_SectionTitle	4	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	{}	f	3	f	6
17	24aa85bb-4f24-4cff-9022-729928c817ff	CarbonNetZero_SectionTitle	4	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	{}	f	3	f	1
18	8616dc4c-d939-438e-a1de-0d8429223315	WelshExclusions_SectionTitle	4	2026-04-15 08:25:31.97341+00	2026-04-15 08:25:31.97341+00	{}	f	4	f	1
19	729e475c-4d53-42a6-8b6a-b32fa7b19567	QualityManagement_SectionTitle	4	2026-04-15 08:25:32.012054+00	2026-04-15 08:25:32.012054+00	{}	f	4	f	2
20	68213741-ab22-4908-8c4c-489147349c3a	WelshSteel_SectionTitle	4	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	{}	f	4	f	4
21	de6eb3d7-c4b5-4315-9392-52b5a936dd7b	Environmental_SectionTitle	4	2026-04-15 08:25:32.189103+00	2026-04-15 08:25:32.189103+00	{}	f	4	f	2
22	a10d1bb8-fe19-45e3-b6e6-c765eeb6f4f0	WelshHealthAndSafety_SectionTitle	4	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	{}	f	4	f	4
\.


--
-- Data for Name: form_answer_sets; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.form_answer_sets (id, guid, section_id, created_on, updated_on, shared_consent_id, deleted, created_from, further_questions_exempted) FROM stdin;
\.


--
-- Data for Name: form_questions; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.form_questions (id, guid, next_question_id, next_question_alternative_id, section_id, type, is_required, title, description, options, created_on, updated_on, caption, summary_title, name, sort_order) FROM stdin;
17	f4a5b6c7-8d9e-0f1a-2b3c-456789def012	16	\N	4	7	t	FinancialInformation_01_Title	\N	{}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	\N	FinancialInformation_01_SummaryTitle	_FinancialInformation01	4
18	e3f4a5b6-7c8d-9e0f-1a2b-3456789cdef1	17	\N	4	2	t	FinancialInformation_02_Title	FinancialInformation_02_Description	{}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	\N	FinancialInformation_02_SummaryTitle	_FinancialInformation02	3
20	c1e2e3f4-5a6b-7c8d-9e0f-123456789abc	19	\N	4	0	t	FinancialInformation_04_Title	FinancialInformation_04_Description	{}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	\N	\N	_FinancialInformation04	1
16	a5b6c7d8-9e0f-1a2b-3c4d-56789ef01234	\N	\N	4	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	\N	\N	_FinancialInformation05	5
19	d2e3f4a5-6b7c-8d9e-0f1a-23456789bcd0	18	\N	4	3	t	FinancialInformation_03_Title	FinancialInformation_03_Description	{}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	\N	FinancialInformation_03_SummaryTitle	_FinancialInformation03	2
25	37e2c348-24cf-42e1-8db6-89522fb13493	24	\N	5	1	t	Global_Enter_Your_Name	ShareMyInformation_04_Description	{}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	Global_Your_Declaration_Details	ShareMyInformation_04_SummaryTitle	_ShareMyInformation04	2
33	be023951-add2-4f19-b422-1e606a2a2479	42	\N	6	3	t	Exclusions_07_Title	\N	{}	2026-04-15 08:25:27.677235+00	2026-04-15 08:25:27.677235+00	\N	Exclusions_07_SummaryTitle	_Exclusion07	1
32	79e7ef12-5796-40a1-84e0-d6a4b6752a6a	31	\N	6	1	t	Exclusions_06_Title	Exclusions_06_Description	{}	2026-04-15 08:25:27.677235+00	2026-04-15 08:25:27.677235+00	\N	Exclusions_06_SummaryTitle	_Exclusion06	4
31	38c35bd8-302a-4fa7-bdc3-dd703380678a	30	\N	6	10	t	Exclusions_05_Title	Exclusions_05_Description	{}	2026-04-15 08:25:27.677235+00	2026-04-15 08:25:27.677235+00	\N	Exclusions_05_SummaryTitle	_Exclusion05	5
30	5aeb23e9-ebba-4c12-825b-b33855a36a08	29	\N	6	10	t	Exclusions_04_Title	Exclusions_04_Description	{}	2026-04-15 08:25:27.677235+00	2026-04-15 08:25:27.677235+00	\N	Exclusions_04_SummaryTitle	_Exclusion04	6
29	d3399169-7e03-43c9-b9a5-8761bb1746ef	43	\N	6	2	f	Exclusions_03_Title	Exclusions_03_Description	{}	2026-04-15 08:25:27.677235+00	2026-04-15 08:25:27.677235+00	Exclusions_03_Caption	Exclusions_03_SummaryTitle	_Exclusion03	7
22	3ca872b0-2d9b-44b0-9f70-a80033a486bf	21	\N	5	9	t	Global_Enter_Your_Postal_Address	ShareMyInformation_01_Description	{"choices": [{"id": "bd4fe649-1f52-429e-978c-472e0a2cf11c", "hint": {"title": null, "description": "Enter the first line of your address."}, "title": "Address line 1", "value": "addressLine1", "groupName": null}, {"id": "a4edc9cd-f198-4e74-88e4-d981b09d30ed", "hint": {"title": null, "description": "Enter the name of your town or city."}, "title": "Town or city", "value": "townOrCity", "groupName": null}, {"id": "ef806205-6e38-4496-9a3e-8bb7a37b48ba", "hint": {"title": null, "description": "Enter your postal code."}, "title": "Post code", "value": "postCode", "groupName": null}], "choiceProviderStrategy": null}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	Global_Your_Declaration_Details	ShareMyInformation_01_SummaryTitle	_ShareMyInformation01	5
28	0a64c578-56dd-45fd-acc7-3b7dac1f5d9e	27	\N	6	7	f	Exclusions_02_Title	Exclusions_02_Description	{}	2026-04-15 08:25:27.677235+00	2026-04-15 08:25:27.677235+00	Exclusions_02_Caption	Exclusions_02_SummaryTitle	_Exclusion02	9
27	99f2af7e-1012-4aca-b6ed-5a9710122d30	\N	\N	6	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:27.677235+00	2026-04-15 08:25:27.677235+00	\N	\N	_Exclusion01	10
46	63339546-55af-4b40-a9cd-3d524a863265	45	\N	9	9	t	Global_Enter_Your_Postal_Address	\N	{"choices": [{"id": "bd4fe649-1f52-429e-978c-472e0a2cf11c", "hint": {"title": null, "description": "Enter the first line of your address."}, "title": "Address line 1", "value": "addressLine1", "groupName": null}, {"id": "a4edc9cd-f198-4e74-88e4-d981b09d30ed", "hint": {"title": null, "description": "Enter the name of your town or city."}, "title": "Town or city", "value": "townOrCity", "groupName": null}, {"id": "ef806205-6e38-4496-9a3e-8bb7a37b48ba", "hint": {"title": null, "description": "Enter your postal code."}, "title": "Post code", "value": "postCode", "groupName": null}], "choiceProviderStrategy": null}	2026-04-15 08:25:29.556123+00	2026-04-15 08:25:29.556123+00	Global_Your_Declaration_Details	ShareMyInformationConsortium_01_SummaryTitle	_ShareMyInformationConsortium01	5
47	0cba4144-6b36-4610-91c5-8ea57fd92a60	46	\N	9	1	t	Global_Enter_Your_Email_Address	\N	{}	2026-04-15 08:25:29.556123+00	2026-04-15 08:25:29.556123+00	Global_Your_Declaration_Details	ShareMyInformationConsortium_02_SummaryTitle	_ShareMyInformationConsortium02	4
48	49f98c72-200f-4e22-8ff7-30cecad6c02c	47	\N	9	1	t	Global_Enter_Your_Job_Title	\N	{}	2026-04-15 08:25:29.556123+00	2026-04-15 08:25:29.556123+00	Global_Your_Declaration_Details	ShareMyInformationConsortium_03_SummaryTitle	_ShareMyInformationConsortium03	3
21	4b1784c6-c916-4de9-aaf9-bc7e26d39e6b	\N	\N	5	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	\N	\N	_ShareMyInformation06	6
49	b6216447-ca62-4fda-bdc7-2a609406d6cd	48	\N	9	1	t	Global_Enter_Your_Name	ShareMyInformationConsortium_04_Description	{}	2026-04-15 08:25:29.556123+00	2026-04-15 08:25:29.556123+00	Global_Your_Declaration_Details	ShareMyInformationConsortium_04_SummaryTitle	_ShareMyInformationConsortium04	2
57	2b122ea1-b98a-4265-a9b8-da0cc88b48f7	51	\N	10	2	f	SteelQuestion_03_Title		{}	2026-04-15 08:25:30.902389+00	2026-04-15 08:25:30.902389+00	\N	SteelQuestion_03_SummaryTitle	_SteelQuestion03	3
51	7f40fb3d-f402-4606-a85b-4115a5f09e63	\N	\N	10	6	t	Global_CheckYourAnswers		{}	2026-04-15 08:25:30.902389+00	2026-04-15 08:25:30.902389+00	\N	\N	_SteelQuestion04	4
43	bf079445-3f3a-4c65-bc37-203e40c5dd2f	28	\N	6	12	f	Exclusions_10_Title	Exclusions_10_Description	{}	2026-04-15 08:25:28.079731+00	2026-04-15 08:25:28.079731+00	Exclusions_10_Caption	Exclusions_10_SummaryTitle	_Exclusion10	8
44	c8951503-bdfa-41c5-b1da-08736c787a49	32	\N	6	4	t	Exclusions_09_Title	Exclusions_09_Description	{"choices": [], "answerFieldName": "JsonValue", "choiceProviderStrategy": "ExclusionAppliesToChoiceProviderStrategy"}	2026-04-15 08:25:28.199972+00	2026-04-15 08:25:28.199972+00	\N	Exclusions_09_SummaryTitle	_Exclusion09	3
45	4c644d8f-b865-4337-8517-f4b8663d6385	\N	\N	9	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:29.556123+00	2026-04-15 08:25:29.556123+00	\N	\N	_ShareMyInformationConsortium06	6
50	b88d244e-2211-4c46-8e58-dddcda68418b	49	\N	9	8	t	Global_Declaration	ShareMyInformationConsortium_05_Description	{"choices": [{"id": "bd4fe649-1f52-429e-978c-472e0a2cf11c", "hint": null, "title": "I understand and agree to the above statements", "value": "agree", "groupName": null}], "choiceProviderStrategy": null}	2026-04-15 08:25:29.556123+00	2026-04-15 08:25:29.556123+00	\N	\N	_ShareMyInformationConsortium05	1
58	f2d4fb34-7b2f-4f42-95ab-98577e35a771	57	\N	10	10	t	SteelQuestion_02_Title	SteelQuestion_02_Description	{}	2026-04-15 08:25:30.902389+00	2026-04-15 08:25:30.902389+00	\N	SteelQuestion_02_SummaryTitle	_SteelQuestion02	2
59	c0e3063b-7334-402d-8a98-aa722a7ea820	58	\N	10	0	t	SteelQuestion_01_Title	SteelQuestion_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:30.902389+00	2026-04-15 08:25:30.902389+00	\N		_SteelQuestion01	1
37	29950eef-eb7d-496d-9a03-8fae586ba415	36	\N	7	1	t	Qualifications_01_Title	Qualifications_01_Description	{}	2026-04-15 08:25:27.71836+00	2026-04-15 08:25:27.71836+00	\N	Qualifications_01_SummaryTitle	_Qualifications01	1
36	34c4031e-6454-4944-8a18-ac15ee2275ea	35	\N	7	1	t	Qualifications_02_Title	Qualifications_02_Description	{}	2026-04-15 08:25:27.71836+00	2026-04-15 08:25:27.71836+00	\N	Qualifications_02_SummaryTitle	_Qualifications02	2
35	5a9cf255-4737-4238-a199-31bbec7b04f4	34	\N	7	7	t	Qualifications_03_Title		{}	2026-04-15 08:25:27.71836+00	2026-04-15 08:25:27.71836+00	\N	Qualifications_03_SummaryTitle	_Qualifications03	3
24	f2e4eb44-9836-4ac7-843a-8f5db5d5cef0	23	\N	5	1	t	Global_Enter_Your_Job_Title	\N	{}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	Global_Your_Declaration_Details	ShareMyInformation_03_SummaryTitle	_ShareMyInformation03	3
26	9012d897-230e-4890-80ba-8b116def0e26	25	\N	5	8	t	Global_Declaration	ShareMyInformation_05_Description	{"choices": [{"id": "bd4fe649-1f52-429e-978c-472e0a2cf11c", "hint": null, "title": "I understand and agree to the above statements", "value": "agree", "groupName": null}], "choiceProviderStrategy": null}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	\N	\N	_ShareMyInformation05	1
42	3d5e2ef2-dfd9-4922-abd0-a2a1fadbb989	44	\N	6	11	t	Exclusions_08_Title	Exclusions_08_Description	{"groups": [{"hint": "Exclusions_08_Options_Groups_01_Hint", "name": "Exclusions_08_Options_Groups_01_Name", "caption": "Exclusions_08_Options_Groups_01_Caption", "choices": [{"id": "7e8669e6-bef3-4cf1-a8c4-a0003f47743a", "title": "Exclusions_08_Options_Groups_01_Choices_01_Title", "value": "adjustments_for_tax_arrangements"}, {"id": "70b33154-689c-499c-be69-abad7e493c92", "title": "Exclusions_08_Options_Groups_01_Choices_02_Title", "value": "competition_law_infringements"}, {"id": "99577e71-1819-4fcf-9e63-1d877dd09c12", "title": "Exclusions_08_Options_Groups_01_Choices_03_Title", "value": "defeat_in_respect"}, {"id": "cf35d4d8-993d-4bea-b14c-b34182df8ebc", "title": "Exclusions_08_Options_Groups_01_Choices_04_Title", "value": "failure_to_cooperate"}, {"id": "629c2ce4-d77e-4fd5-ac78-1dd1c1747145", "title": "Exclusions_08_Options_Groups_01_Choices_05_Title", "value": "finding_by_HMRC"}, {"id": "8dd0e803-3158-4fc6-8047-683a68c2ee8e", "title": "Exclusions_08_Options_Groups_01_Choices_06_Title", "value": "penalties_for_transactions"}, {"id": "2847dc04-c6f5-4f87-abb3-0c6884844c59", "title": "Exclusions_08_Options_Groups_01_Choices_07_Title", "value": "penalties_payable"}]}, {"hint": "Exclusions_08_Options_Groups_02_Hint", "name": "Exclusions_08_Options_Groups_02_Name", "caption": "Exclusions_08_Options_Groups_02_Caption", "choices": [{"id": "94f56050-a879-4de4-967d-1e9b3ee9ee70", "title": "Exclusions_08_Options_Groups_02_Choices_01_Title", "value": "ancillary_offences_aiding"}, {"id": "635c1f29-1b77-4e5f-ab47-039fff1eb8d1", "title": "Exclusions_08_Options_Groups_02_Choices_02_Title", "value": "cartel_offences"}, {"id": "46277494-f562-4c82-8b92-c52db58bbaee", "title": "Exclusions_08_Options_Groups_02_Choices_03_Title", "value": "corporate_manslaughter"}, {"id": "d2cd6943-af69-4ed4-9ba3-1bf351bf6a3a", "title": "Exclusions_08_Options_Groups_02_Choices_04_Title", "value": "labour_market"}, {"id": "88cf8c4b-309a-4764-92fc-2fcb03efc9c7", "title": "Exclusions_08_Options_Groups_02_Choices_05_Title", "value": "organised_crime"}, {"id": "e6976075-5b3b-4682-b74e-f4f37f4e314e", "title": "Exclusions_08_Options_Groups_02_Choices_06_Title", "value": "tax_offences"}, {"id": "c4c658fe-4cbe-45a8-bd2a-ea2c8e20d7b4", "title": "Exclusions_08_Options_Groups_02_Choices_07_Title", "value": "terrorism_and_offences"}, {"id": "fb0f171b-4fb4-4ee6-ab7d-c2cca4be948c", "title": "Exclusions_08_Options_Groups_02_Choices_08_Title", "value": "theft_fraud"}]}, {"hint": "Exclusions_08_Options_Groups_03_Hint", "name": "Exclusions_08_Options_Groups_03_Name", "caption": "Exclusions_08_Options_Groups_03_Caption", "choices": [{"id": "bac47f5c-cb99-4681-af19-a01a7bbee86a", "title": "Exclusions_08_Options_Groups_03_Choices_01_Title", "value": "acting_improperly"}, {"id": "4cd39237-88d2-4b4d-a037-1d2cc76ce869", "title": "Exclusions_08_Options_Groups_03_Choices_02_Title", "value": "breach_of_contract"}, {"id": "cc8bbedb-c698-42e6-a217-f37cf65f128e", "title": "Exclusions_08_Options_Groups_03_Choices_03_Title", "value": "environmental_misconduct"}, {"id": "0e3c0e11-d908-46fe-ab40-faf32833a825", "title": "Exclusions_08_Options_Groups_03_Choices_04_Title", "value": "infringement_of_competition"}, {"id": "df0f6ccc-8917-4ccb-a3b5-ff4d5da9b69d", "title": "Exclusions_08_Options_Groups_03_Choices_05_Title", "value": "insolvency_bankruptcy"}, {"id": "60fd754f-a4d7-494b-8717-068924e7017b", "title": "Exclusions_08_Options_Groups_03_Choices_06_Title", "value": "labour_market_misconduct"}, {"id": "8c668f5f-21f2-4004-b6ba-f62fab845d06", "title": "Exclusions_08_Options_Groups_03_Choices_07_Title", "value": "potential_competition_law_infringements"}, {"id": "a76aee38-2306-4b75-a082-2513ac142b4c", "title": "Exclusions_08_Options_Groups_03_Choices_08_Title", "value": "professional_misconduct"}, {"id": "a9843a24-ea46-4c96-9a4f-0c829d251db4", "title": "Exclusions_08_Options_Groups_03_Choices_09_Title", "value": "substantial_part_business"}]}], "choices": null, "choiceProviderStrategy": null}	2026-04-15 08:25:27.98401+00	2026-04-15 08:25:27.98401+00	\N	Exclusions_08_SummaryTitle	_Exclusion08	2
34	f16f6b6b-229c-4394-9048-8e2ac8bc8496	\N	\N	7	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:27.71836+00	2026-04-15 08:25:27.71836+00	\N	\N	_Qualifications04	4
41	b788af54-2e40-4200-ad98-bd44d612c323	40	\N	8	1	t	TradeAssurance_01_Title	TradeAssurance_01_Description	{}	2026-04-15 08:25:27.777286+00	2026-04-15 08:25:27.777286+00	\N	TradeAssurance_01_SummaryTitle	_TradeAssurance01	1
40	daebd86e-3533-4edc-9de8-300a0ebd6d02	39	\N	8	1	f	TradeAssurance_02_Title	\N	{}	2026-04-15 08:25:27.777286+00	2026-04-15 08:25:27.777286+00	TradeAssurance_02_Caption	TradeAssurance_02_SummaryTitle	_TradeAssurance02	2
39	63737d99-51f5-46d8-9ca2-99fe5dbd7765	38	\N	8	7	t	TradeAssurance_03_Title		{}	2026-04-15 08:25:27.777286+00	2026-04-15 08:25:27.777286+00	\N	TradeAssurance_03_SummaryTitle	_TradeAssurance03	3
38	b5896eb5-e79a-4927-9e29-3258e997b742	\N	\N	8	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:27.777286+00	2026-04-15 08:25:27.777286+00	\N	\N	_TradeAssurance04	4
23	265a61a0-3370-4203-884f-7be7a6571210	22	\N	5	1	t	Global_Enter_Your_Email_Address	ShareMyInformation_02_Description	{}	2026-04-15 08:25:27.076858+00	2026-04-15 08:25:27.076858+00	Global_Your_Declaration_Details	ShareMyInformation_02_SummaryTitle	_ShareMyInformation02	4
74	5fe04d88-389e-4129-b7bb-647e902d6703	\N	\N	12	6	t	Global_CheckYourAnswers		{}	2026-04-15 08:25:30.978512+00	2026-04-15 08:25:30.978512+00	\N	\N	_HealthAndSafetyQuestion04	4
77	d938b486-27f9-48ab-a2b6-095ec46b1a4f	76	\N	12	0	t	HealthAndSafetyQuestion_01_Title	HealthAndSafetyQuestion_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:30.978512+00	2026-04-15 08:25:30.978512+00	\N		_HealthAndSafetyQuestion01	1
76	7ac2fd79-f13c-45b0-b5cd-0d97975f1976	75	\N	12	10	t	HealthAndSafetyQuestion_02_Title	HealthAndSafetyQuestion_02_Description	{"layout": {"button": {"beforeButtonContent": "HealthAndSafetyQuestion_02_CustomInsetText"}}}	2026-04-15 08:25:30.978512+00	2026-04-15 08:25:30.978512+00	\N	HealthAndSafetyQuestion_02_SummaryTitle	_HealthAndSafetyQuestion02	2
75	76a4bb3a-06c9-4af1-b9dd-288708da1e71	74	\N	12	2	f	HealthAndSafetyQuestion_03_Title	HealthAndSafetyQuestion_03_Description	{}	2026-04-15 08:25:30.978512+00	2026-04-15 08:25:30.978512+00	\N	HealthAndSafetyQuestion_03_SummaryTitle	_HealthAndSafetyQuestion03	3
90	6a87e916-e96d-4614-8ae5-3bd505725fc4	\N	\N	13	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N		_ModernSlaveryQuestion13	13
79	70261061-2ce7-4383-8b3a-0bf90bafea10	80	81	13	3	t	ModernSlavery_02_Title	ModernSlavery_02_Description	{}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N	ModernSlavery_02_SummaryTitle	_ModernSlaveryQuestion02	2
80	16834d2a-2767-49d8-bb90-1a5ed62ec023	82	83	13	3	t	ModernSlavery_03_Title		{}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N	ModernSlavery_03_SummaryTitle	_ModernSlaveryQuestion03	3
81	10fa434b-b00b-4a63-9e30-98fc82ee8d41	84	85	13	3	t	ModernSlavery_04_Title	ModernSlavery_04_Description	{}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N	ModernSlavery_04_SummaryTitle	_ModernSlaveryQuestion04	4
88	36f3831c-0519-4099-9fba-1fdb1c40b675	90	\N	13	10	t	ModernSlavery_11_Title	ModernSlavery_11_Description	{}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N	ModernSlavery_11_SummaryTitle	_ModernSlaveryQuestion11	11
89	33b28d67-8fee-4ebc-8dbe-cb5ac2967a34	90	\N	13	10	t	ModernSlavery_12_Title	ModernSlavery_12_Description	{}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N	ModernSlavery_12_SummaryTitle	_ModernSlaveryQuestion12	12
100	bafae07c-10ab-486c-aff3-e171bc8d2625	\N	\N	14	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	\N	_CyberEssentials10	10
106	108fb27d-908f-44c9-a0a7-63892b9aace6	\N	\N	15	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:31.226056+00	2026-04-15 08:25:31.226056+00	\N		_DataProtectionQuestion06	6
104	aced113e-fa13-4399-83b8-821e7f87724f	105	\N	15	10	t	DataProtection_04_Title	DataProtection_04_Description	{}	2026-04-15 08:25:31.226056+00	2026-04-15 08:25:31.226056+00	\N	DataProtection_04_SummaryTitle	_DataProtectionQuestion04	4
105	e2942d66-63d0-45da-917c-1fa3e4e0374c	106	106	15	2	f	DataProtection_05_Title	\N	{}	2026-04-15 08:25:31.226056+00	2026-04-15 08:25:31.226056+00	\N	DataProtection_05_SummaryTitle	_DataProtectionQuestion05	5
78	6ff882a9-8d24-48ab-a18d-2a1e0f81d6a3	79	\N	13	0	t	ModernSlavery_01_Title	ModernSlavery_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N		_ModernSlaveryQuestion01	1
91	ec7077ed-a41d-4f2e-8761-f980f27dda42	92	\N	14	0	t	CyberEssentials_01_Title	CyberEssentials_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	\N	_CyberEssentials01	1
101	bd96170d-87ba-4bcc-a993-d7dfb6ffc97a	102	\N	15	0	t	DataProtection_01_Title	DataProtection_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:31.226056+00	2026-04-15 08:25:31.226056+00	\N		_DataProtectionQuestion01	1
107	d9c56cf3-2a5c-45a0-9c12-2d03febad649	108	\N	16	0	t	Payments_01_Title	Payments_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N		_Payments01	1
82	ec703401-a2b5-4cd8-b2da-6cb160acd302	89	\N	13	12	t	ModernSlavery_05_Title	ModernSlavery_05_Description	{}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N	ModernSlavery_05_SummaryTitle	_ModernSlaveryQuestion05	5
108	8a65c39d-d051-47c1-8e8b-f209e3a84e0b	109	\N	16	3	t	Payments_02_Title	Payments_02_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789001", "page": false, "summaryTitle": "Payments_Group1_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_02_SummaryTitle	_Payments02	2
109	275da7fe-2122-4401-b4d9-860d3a724239	110	\N	16	3	t	Payments_03_Title	Payments_03_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789001", "page": false, "summaryTitle": "Payments_Group1_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_03_SummaryTitle	_Payments03	3
110	26950cec-4cd3-41fd-9d45-1c002d49296a	111	\N	16	3	t	Payments_04_Title	Payments_04_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789001", "page": false, "summaryTitle": "Payments_Group1_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_04_SummaryTitle	_Payments04	4
84	d3884626-29d8-4be1-9832-4fad898c1e10	88	\N	13	12	t	ModernSlavery_07_Title	\N	{}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N	ModernSlavery_07_SummaryTitle	_ModernSlaveryQuestion07	7
102	5fa72124-adde-4a2d-bde9-b02f7dcfac3b	103	104	15	3	t	DataProtection_02_Title	DataProtection_02_Description	{"layout": {"input": {"customNoText": "DataProtection_02_Custom_No", "customYesText": "DataProtection_02_Custom_Yes"}}}	2026-04-15 08:25:31.226056+00	2026-04-15 08:25:31.226056+00	\N	DataProtection_02_SummaryTitle	_DataProtectionQuestion02	2
83	cd0ebe51-22d6-464b-abed-eeb83ca68416	89	\N	13	2	f	ModernSlavery_06_Title		{"layout": {"button": {"beforeButtonContent": "ModernSlavery_06_CustomInsetText"}}}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N	ModernSlavery_06_SummaryTitle	_ModernSlaveryQuestion06	6
85	6f126639-186e-4742-995b-4693e575f192	88	\N	13	2	f	ModernSlavery_08_Title	\N	{"layout": {"button": {"beforeButtonContent": "ModernSlavery_08_CustomInsetText"}}}	2026-04-15 08:25:31.096343+00	2026-04-15 08:25:31.096343+00	\N	ModernSlavery_08_SummaryTitle	_ModernSlaveryQuestion08	8
132	120cce73-cf94-498a-ac76-af723637ba49	\N	\N	16	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N		_Payments12	26
146	37295e92-c74b-44b9-a7e0-de0e998420ce	\N	\N	17	6	t	Global_CheckYourAnswers	\N	{}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	\N	_CarbonNetZeroQuestion14	14
133	62e1484e-1a43-46b1-8e77-a44d36bc84c0	134	\N	17	0	t	CarbonNetZero_01_Title	CarbonNetZero_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	CarbonNetZero_01_SummaryTitle	_CarbonNetZeroQuestion01	1
176	c7194e75-4f9f-4c25-8b3d-3e66d7ea2e67	177	\N	22	0	t	WelshHealthAndSafetyQuestion_01_Title	WelshHealthAndSafetyQuestion_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_01_SummaryTitle	_WelshHealthAndSafetyQuestion01	1
96	7c0d6406-147b-4395-895a-7efcaa05e8f9	97	100	14	3	t	CyberEssentials_06_Title	\N	{"layout": {"input": {"customNoText": "CyberEssentials_06_CustomNoText", "customYesText": "CyberEssentials_06_CustomYesText"}}, "grouping": {"id": "b2c3d4e5-f6a7-4901-bcde-f23456789012", "page": false, "summaryTitle": "CyberEssentials_Group2_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	CyberEssentials_06_SummaryTitle	_CyberEssentials06	6
95	b4d0f077-b448-4af4-9f9a-3402ababe3a9	96	\N	14	7	t	CyberEssentials_05_Title	\N	{"layout": {"input": {"customNoText": "CyberEssentials_05_CustomNoText", "customYesText": "CyberEssentials_05_CustomYesText"}}, "grouping": {"id": "a1b2c3d4-e5f6-4789-abcd-ef1234567890", "page": false, "checkYourAnswers": true}, "validation": {"dateValidationType": "FutureOnly"}}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	CyberEssentials_05_SummaryTitle	_CyberEssentials05	5
169	381a2ac4-e13c-42ea-b42c-d9c58df37849	\N	\N	20	6	t	Global_CheckYourAnswers		{}	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	\N	\N	_WelshSteel09	9
164	add9f774-4c83-48b5-9a2b-1e99ae4790f3	166	165	20	2	f	WelshSteel_04_Title	WelshSteel_04_Description	{"layout": {"button": {"beforeButtonContent": "WelshSteel_04_CustomInsetText"}}}	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	\N	WelshSteel_04_SummaryTitle	_WelshSteel04	4
165	d2669b4f-06ef-4f2f-b9dd-eac5b9375a42	166	\N	20	10	t	WelshSteel_05_Title	WelshSteel_05_Description	{}	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	\N	WelshSteel_05_SummaryTitle	_WelshSteel05	5
97	2c9eecae-4dd4-4df7-837e-de0f5c0c5f16	98	100	14	3	t	CyberEssentials_07_Title	\N	{"layout": {"input": {"customNoText": "CyberEssentials_07_CustomNoText", "customYesText": "CyberEssentials_07_CustomYesText"}}, "grouping": {"id": "b2c3d4e5-f6a7-4901-bcde-f23456789012", "page": false, "checkYourAnswers": true}}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	CyberEssentials_07_SummaryTitle	_CyberEssentials07	7
170	dfc1df3e-ca46-44e2-9edb-9b989745c24d	\N	\N	21	6	t	Global_CheckYourAnswers		{}	2026-04-15 08:25:32.189103+00	2026-04-15 08:25:32.189103+00	\N	\N	_EnvironmentalQuestion06	6
175	892621bd-37b5-4218-8258-1fc1b9201aa1	174	\N	21	0	t	EnvironmentalQuestion_01_Title	EnvironmentalQuestion_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:32.189103+00	2026-04-15 08:25:32.189103+00	\N		_EnvironmentalQuestion01	1
166	500dc525-afb6-4f2e-b59c-c069a85a5056	167	\N	20	10	f	WelshSteel_06_Title	WelshSteel_06_Description	{}	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	WelshSteel_06_Caption	WelshSteel_06_SummaryTitle	_WelshSteel06	6
167	8f542fcf-6170-4f92-b5a1-9dce41a3098d	168	\N	20	10	f	WelshSteel_07_Title	WelshSteel_07_Description	{}	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	WelshSteel_07_Caption	WelshSteel_07_SummaryTitle	_WelshSteel07	7
168	a27f6f12-5654-453b-98c6-be11a00f021c	169	\N	20	10	f	WelshSteel_08_Title	WelshSteel_08_Description	{}	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	WelshSteel_08_Caption	WelshSteel_08_SummaryTitle	_WelshSteel08	8
173	46c2a9f7-79ef-416c-b092-1bbc127103e4	172	\N	21	2	f	EnvironmentalQuestion_03_Title	\N	{"layout": {"button": {"beforeButtonContent": "EnvironmentalQuestion_03_CustomInsetText"}}}	2026-04-15 08:25:32.189103+00	2026-04-15 08:25:32.189103+00	\N	EnvironmentalQuestion_03_SummaryTitle	_EnvironmentalQuestion03	3
171	1b021485-b4a7-4de9-8a70-b270ded7d10e	170	\N	21	2	f	EnvironmentalQuestion_05_Title	\N	{"layout": {"button": {"beforeButtonContent": "EnvironmentalQuestion_05_CustomInsetText"}}}	2026-04-15 08:25:32.189103+00	2026-04-15 08:25:32.189103+00	\N	EnvironmentalQuestion_05_SummaryTitle	_EnvironmentalQuestion05	5
174	c6e1e587-921d-4454-870b-2008d64b7dd0	173	172	21	3	f	EnvironmentalQuestion_02_Title	\N	{"layout": {"button": {"beforeButtonContent": "EnvironmentalQuestion_02_CustomInsetText"}}}	2026-04-15 08:25:32.189103+00	2026-04-15 08:25:32.189103+00	\N	EnvironmentalQuestion_02_SummaryTitle	_EnvironmentalQuestion02	2
172	d22b68fc-7324-44d9-985e-a28d6af46753	171	170	21	3	f	EnvironmentalQuestion_04_Title	\N	{}	2026-04-15 08:25:32.189103+00	2026-04-15 08:25:32.189103+00	\N	EnvironmentalQuestion_04_SummaryTitle	_EnvironmentalQuestion04	4
129	cf25e7d3-cc2c-44e3-9256-ccc4c46288c5	130	131	16	3	t	Payments_09_Title	Payments_09_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789004", "page": false, "summaryTitle": "Payments_Group4_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_09_SummaryTitle	_Payments09	23
115	42f7c133-71dd-4406-9d79-94af8b436517	116	\N	16	0	t	Payments_06_InvoicesPaid_Label	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": true, "summaryTitle": "Payments_Group2_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N		_Payments06_3	9
123	c4004e62-72dd-4735-bdc2-b0977dd004e7	124	\N	16	0	t	Payments_07_InvoicesPaid_Label	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": true, "summaryTitle": "Payments_Group3_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N		_Payments07_3	17
111	f261c3f7-4ff7-4fc7-beb2-2882cf3bb888	112	\N	16	12	f	Payments_05_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789001", "page": false, "summaryTitle": "Payments_Group1_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	Payments_05_Caption	Payments_05_SummaryTitle	_Payments05	5
112	d5337691-b75d-4269-b6f5-a87b43d91c63	113	\N	16	0	t	Payments_06_Title	Payments_06_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": true, "summaryTitle": "Payments_Group2_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	Payments_06_Caption		_Payments06	6
120	645d96e3-6a62-437a-a121-5c8b6d7fc703	121	\N	16	0	t	Payments_07_Title	Payments_07_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": true, "summaryTitle": "Payments_Group3_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	Payments_07_Caption		_Payments07	14
113	55aaa76f-b84f-4864-9680-330da99f5330	114	\N	16	7	t	Payments_06_ReportingStartDate	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": true, "summaryTitle": "Payments_Group2_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	<span class="govuk-!-display-none"></span>	Payments_06_ReportingStartDate_SummaryTitle	_Payments06_1	7
121	c49384f3-8f28-451a-ad89-f9e6cc7812f4	122	\N	16	7	t	Payments_07_ReportingStartDate	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": true, "summaryTitle": "Payments_Group3_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	<span class="govuk-!-display-none"></span>	Payments_07_ReportingStartDate_SummaryTitle	_Payments07_1	15
128	04383015-6280-4961-8a6d-e09c113e4ba9	129	\N	16	10	t	Payments_08_Title	Payments_08_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789004", "page": false, "summaryTitle": "Payments_Group4_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_08_SummaryTitle	_Payments08	22
130	c94fd2ae-e947-43ad-8134-8e7f94aecda8	132	\N	16	3	t	Payments_10_Title	Payments_10_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789004", "page": false, "summaryTitle": "Payments_Group4_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_10_SummaryTitle	_Payments10	24
136	27d06ad3-452c-40fe-aac4-c7a0496d7330	137	\N	17	7	t	CarbonNetZero_04_Title	\N	{"grouping": {"id": "b954f64b-2c6b-4fbf-94f0-baed937b7c61", "page": false, "summaryTitle": "CarbonNetZero_CarbonReductionPlan", "checkYourAnswers": true}, "validation": {"dateValidationType": "FutureOnly"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	DateForm_FutureDate_Hint	\N	_CarbonNetZeroQuestion04	4
147	c5837cbe-089b-4b5f-b248-a5ac5d25bd95	148	\N	17	0	f	CarbonNetZero_06_BaselineContainer_Title	\N	{"grouping": {"id": "75e44cb9-6cca-4f88-94d1-99cd4b8631ac", "page": true, "summaryTitle": "CarbonNetZero_BaselineEmissions_Group", "checkYourAnswers": true}}	2026-04-15 08:25:31.589119+00	2026-04-15 08:25:31.589119+00	CarbonNetZero_CarbonNetZeroDetails_SupplierEmissionsDeclaration_Text	\N	_CarbonNetZeroBaselineContainer	5
148	d570dd4e-33ed-4ef7-ba7b-52640c3be54a	138	\N	17	0	f	CarbonNetZero_CarbonNetZeroDetails_RelevantEmissionsData_Heading	\N	{"grouping": {"id": "75e44cb9-6cca-4f88-94d1-99cd4b8631ac", "page": true, "summaryTitle": "CarbonNetZero_BaselineEmissions_Group", "checkYourAnswers": true}}	2026-04-15 08:25:31.589119+00	2026-04-15 08:25:31.589119+00	\N	\N	_CarbonNetZeroBaselineLabel	5
135	b2550646-820e-4afb-8dfe-0b50a33ebc7f	136	\N	17	12	f	CarbonNetZero_03_Title	\N	{"grouping": {"id": "b954f64b-2c6b-4fbf-94f0-baed937b7c61", "page": false, "summaryTitle": "CarbonNetZero_CarbonReductionPlan", "checkYourAnswers": true}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	CarbonNetZero_03_Caption	\N	_CarbonNetZeroQuestion03	3
134	66e74e47-3211-4126-b849-76ac4a1fe8e0	135	\N	17	3	t	CarbonNetZero_02_Title	\N	{"grouping": {"id": "b954f64b-2c6b-4fbf-94f0-baed937b7c61", "page": false, "summaryTitle": "CarbonNetZero_CarbonReductionPlan", "checkYourAnswers": true}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	\N	_CarbonNetZeroQuestion02	2
137	8d84975a-611f-4de0-a946-468d25d0c2d5	147	\N	17	1	t	CarbonNetZero_05_Title	CarbonNetZero_05_Description	{"layout": {"input": {"width": "3"}}, "grouping": {"id": "b954f64b-2c6b-4fbf-94f0-baed937b7c61", "page": false, "summaryTitle": "CarbonNetZero_CarbonReductionPlan", "checkYourAnswers": true}, "validation": {"textValidationType": "Year"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	\N	_CarbonNetZeroQuestion05	5
138	0beeb82b-5ef2-4989-9712-215e56293f33	139	\N	17	1	t	CarbonNetZero_06_Title		{"layout": {"input": {"width": "3"}}, "grouping": {"id": "75e44cb9-6cca-4f88-94d1-99cd4b8631ac", "page": true, "summaryTitle": "CarbonNetZero_BaselineEmissions_Group", "checkYourAnswers": true}, "validation": {"textValidationType": "Year"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	CarbonNetZero_06_SummaryTitle	_CarbonNetZeroQuestion06	6
139	86bc7b9a-03bb-4405-a227-3761666d3b9b	140	\N	17	1	t	CarbonNetZero_07_Title		{"layout": {"input": {"width": "3", "suffix": {"text": "CarbonNetZero_tCO2e", "type": 0}}}, "grouping": {"id": "75e44cb9-6cca-4f88-94d1-99cd4b8631ac", "page": true, "summaryTitle": "CarbonNetZero_BaselineEmissions_Group", "checkYourAnswers": true}, "validation": {"textValidationType": "Decimal"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	CarbonNetZero_07_SummaryTitle	_CarbonNetZeroQuestion07	7
140	10dd7dec-214d-40dc-976d-09db5bc02625	141	\N	17	1	t	CarbonNetZero_08_Title		{"layout": {"input": {"width": "3", "suffix": {"text": "CarbonNetZero_tCO2e", "type": 0}}}, "grouping": {"id": "75e44cb9-6cca-4f88-94d1-99cd4b8631ac", "page": true, "summaryTitle": "CarbonNetZero_BaselineEmissions_Group", "checkYourAnswers": true}, "validation": {"textValidationType": "Decimal"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	CarbonNetZero_08_SummaryTitle	_CarbonNetZeroQuestion08	8
149	ed20e903-6df9-4ae0-a873-6498e6a08bc7	150	\N	17	0	f	CarbonNetZero_10_ReportingContainer_Title	\N	{"grouping": {"id": "e1df7ec0-79de-4ad9-aabf-e8d93304040b", "page": true, "summaryTitle": "CarbonNetZero_ReportingEmissions_Group", "checkYourAnswers": true}}	2026-04-15 08:25:31.589119+00	2026-04-15 08:25:31.589119+00	CarbonNetZero_CarbonNetZeroDetails_SupplierEmissionsDeclaration_Text	\N	_CarbonNetZeroReportingContainer	9
150	1874ea6a-65ea-4f6f-8ab3-55bd04963d72	142	\N	17	0	f	CarbonNetZero_CarbonNetZeroDetails_RelevantEmissionsData_Heading	\N	{"grouping": {"id": "e1df7ec0-79de-4ad9-aabf-e8d93304040b", "page": true, "summaryTitle": "CarbonNetZero_ReportingEmissions_Group", "checkYourAnswers": true}}	2026-04-15 08:25:31.589119+00	2026-04-15 08:25:31.589119+00	\N	\N	_CarbonNetZeroReportingLabel	9
103	f163ebf6-3cf5-410b-a53c-9c2290b021d8	104	\N	15	3	t	DataProtection_03_Title	DataProtection_03_Description	{"layout": {"input": {"customNoText": "DataProtection_03_Custom_No", "customYesText": "DataProtection_03_Custom_Yes"}}}	2026-04-15 08:25:31.226056+00	2026-04-15 08:25:31.226056+00	\N	DataProtection_03_SummaryTitle	_DataProtectionQuestion03	3
98	cb30f517-5578-47f2-ab42-18c08362b082	99	\N	14	2	t	CyberEssentials_08_Title	\N	{"layout": {"input": {"customNoText": "CyberEssentials_08_CustomNoText", "customYesText": "CyberEssentials_08_CustomYesText"}}, "grouping": {"id": "b2c3d4e5-f6a7-4901-bcde-f23456789012", "page": false, "checkYourAnswers": true}}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	CyberEssentials_08_SummaryTitle	_CyberEssentials08	8
145	be482391-0b35-4a07-8474-070e755f75e5	146	\N	17	1	t	CarbonNetZero_13_Title		{"layout": {"input": {"width": "3", "suffix": {"text": "CarbonNetZero_tCO2e", "type": 0}}}, "grouping": {"id": "e1df7ec0-79de-4ad9-aabf-e8d93304040b", "page": true, "summaryTitle": "CarbonNetZero_ReportingEmissions_Group", "checkYourAnswers": true}, "validation": {"textValidationType": "Decimal"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	CarbonNetZero_13_SummaryTitle	_CarbonNetZeroQuestion13	13
114	09afabe9-490e-47b2-a814-aea0810d892c	115	\N	16	1	t	Payments_06_AverageDaysToPayInvoice	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "Payments_Days", "type": 1}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": true, "summaryTitle": "Payments_Group2_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Number"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_06_AverageDaysToPayInvoice_SummaryTitle	_Payments06_2	8
92	cdc4f79e-6a26-44c9-aef8-066da3aa357f	93	100	14	3	t	CyberEssentials_02_Title	\N	{"layout": {"input": {"customNoText": "CyberEssentials_02_CustomNoText", "customYesText": "CyberEssentials_02_CustomYesText"}}, "grouping": {"id": "a1b2c3d4-e5f6-4789-abcd-ef1234567890", "page": false, "summaryTitle": "CyberEssentials_Group1_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	CyberEssentials_02_SummaryTitle	_CyberEssentials02	2
93	96b12d8f-297e-4a09-87d8-a5c17109e6d5	94	96	14	3	t	CyberEssentials_03_Title	\N	{"layout": {"input": {"customNoText": "CyberEssentials_03_CustomNoText", "customYesText": "CyberEssentials_03_CustomYesText"}}, "grouping": {"id": "a1b2c3d4-e5f6-4789-abcd-ef1234567890", "page": false, "checkYourAnswers": true}}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	CyberEssentials_03_SummaryTitle	_CyberEssentials03	3
99	42034723-44d6-479d-a153-e4097e359bdd	100	\N	14	7	t	CyberEssentials_09_Title	\N	{"layout": {"input": {"customNoText": "CyberEssentials_09_CustomNoText", "customYesText": "CyberEssentials_09_CustomYesText"}}, "grouping": {"id": "b2c3d4e5-f6a7-4901-bcde-f23456789012", "page": false, "checkYourAnswers": true}, "validation": {"dateValidationType": "FutureOnly"}}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	CyberEssentials_09_SummaryTitle	_CyberEssentials09	9
151	5a5870db-c3c0-4db5-bdee-776007d4bc8f	152	\N	18	0	t	WelshExclusions_01_Title	WelshExclusions_01_Description	{}	2026-04-15 08:25:31.97341+00	2026-04-15 08:25:31.97341+00	\N		_WelshExclusions01	1
160	2f4bb075-540a-45fa-ac8d-9399722df0b6	159	\N	19	0	t	QualityManagementQuestion_01_Title	QualityManagementQuestion_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:32.012054+00	2026-04-15 08:25:32.012054+00	\N		_QualityManagementQuestion01	1
161	51885c72-41b5-4e63-b594-510602db01fc	162	\N	20	0	f	WelshSteel_01_Title	WelshSteel_01_Description	{"layout": {"button": {"text": "Global_Start", "style": "Start"}}}	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	\N	WelshSteel_01_SummaryTitle	_WelshSteel01	1
162	fab57d8a-796b-4028-8890-306cab4e99b3	164	163	20	2	f	WelshSteel_02_Title	WelshSteel_02_Description	{"layout": {"button": {"beforeButtonContent": "WelshSteel_02_CustomInsetText"}}}	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	\N	WelshSteel_02_SummaryTitle	_WelshSteel02	2
163	91a306be-f390-44fe-ae27-05346224ea29	164	\N	20	10	t	WelshSteel_03_Title	WelshSteel_03_Description	{}	2026-04-15 08:25:32.130603+00	2026-04-15 08:25:32.130603+00	\N	WelshSteel_03_SummaryTitle	_WelshSteel03	3
157	4ce2ac35-38ed-4659-86fb-0201920e310d	\N	\N	19	6	t	Global_CheckYourAnswers		{}	2026-04-15 08:25:32.012054+00	2026-04-15 08:25:32.012054+00	\N	\N	_QualityManagementQuestion05	5
158	5b48c273-0c8e-44ef-8eb1-019926340be2	157	\N	19	3	t	QualityManagementQuestion_04_Title	\N	{}	2026-04-15 08:25:32.012054+00	2026-04-15 08:25:32.012054+00	\N	QualityManagementQuestion_04_SummaryTitle	_QualityManagementQuestion04	4
159	e0a35463-6869-4ea5-9731-76078efd5a42	197	158	19	3	f	QualityManagementQuestion_02_Title	QualityManagementQuestion_02_Description	{}	2026-04-15 08:25:32.012054+00	2026-04-15 08:25:32.012054+00	\N	QualityManagementQuestion_02_SummaryTitle	_QualityManagementQuestion02	2
152	66ebcacf-5584-4a24-8e1e-1c410281ee12	153	156	18	3	t	WelshExclusions_02_Title	WelshExclusions_02_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789001", "page": false, "summaryTitle": "WelshExclusions_Group1_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.97341+00	2026-04-15 08:25:31.97341+00	\N	WelshExclusions_02_SummaryTitle	_WelshExclusions02	2
153	7acb32db-7d1a-4867-9273-4a36e9916192	154	\N	18	10	t	WelshExclusions_03_Title	WelshExclusions_03_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789002", "page": false, "summaryTitle": "WelshExclusions_Group2_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.97341+00	2026-04-15 08:25:31.97341+00	\N	WelshExclusions_03_SummaryTitle	_WelshExclusions03	3
131	1856f871-8b0c-4c3b-b9f7-c52165baaba6	130	\N	16	2	t	Payments_11_Title	Payments_11_Description	{"layout": {}, "grouping": {"id": "12345678-1234-1234-1234-123456789004", "page": false, "summaryTitle": "Payments_Group4_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_11_SummaryTitle	_Payments11	25
94	71124254-d834-49e9-b837-b2abef71e1b6	95	\N	14	2	t	CyberEssentials_04_Title	\N	{"layout": {"input": {"customNoText": "CyberEssentials_04_CustomNoText", "customYesText": "CyberEssentials_04_CustomYesText"}}, "grouping": {"id": "a1b2c3d4-e5f6-4789-abcd-ef1234567890", "page": false, "checkYourAnswers": true}}	2026-04-15 08:25:31.143371+00	2026-04-15 08:25:31.143371+00	\N	CyberEssentials_04_SummaryTitle	_CyberEssentials04	4
116	13775969-f43c-430d-8fe3-ac52aa1dc867	117	\N	16	1	t	Payments_06_PctPaidWithin30Days	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "%", "type": 0}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": true, "summaryTitle": "Payments_Group2_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Percentage"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_06_PctPaidWithin30Days_SummaryTitle	_Payments06_4	10
117	f9c8e797-f296-4101-8667-0d0acbf70d24	118	\N	16	1	t	Payments_06_PctPaid31To60Days	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "%", "type": 0}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": true, "summaryTitle": "Payments_Group2_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Percentage"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_06_PctPaid31To60Days_SummaryTitle	_Payments06_5	11
118	21e7104b-249d-4db8-8be0-a38d8d93f726	119	\N	16	1	t	Payments_06_PctPaid61OrMoreDays	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "%", "type": 0}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": true, "summaryTitle": "Payments_Group2_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Percentage"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_06_PctPaid61OrMoreDays_SummaryTitle	_Payments06_6	12
119	a9df3e7a-874f-4e5e-805d-a2bf6f7792bd	120	\N	16	1	t	Payments_06_PctPaidOverdue	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "%", "type": 0}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": true, "summaryTitle": "Payments_Group2_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Percentage"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_06_PctPaidOverdue_SummaryTitle	_Payments06_7	13
122	83b78f87-59c5-4de9-ad90-9ae186934301	123	\N	16	1	t	Payments_07_AverageDaysToPayInvoice	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "Payments_Days", "type": 1}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": true, "summaryTitle": "Payments_Group3_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Number"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_07_AverageDaysToPayInvoice_SummaryTitle	_Payments07_2	16
142	fee53ca0-0b42-45fc-b178-e082e2e7c6cb	143	\N	17	1	t	CarbonNetZero_10_Title		{"layout": {"input": {"width": "3"}}, "grouping": {"id": "e1df7ec0-79de-4ad9-aabf-e8d93304040b", "page": true, "summaryTitle": "CarbonNetZero_ReportingEmissions_Group", "checkYourAnswers": true}, "validation": {"textValidationType": "Year"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	CarbonNetZero_10_SummaryTitle	_CarbonNetZeroQuestion10	10
124	2014c5f7-045a-46ea-a901-a0da5efaf427	125	\N	16	1	t	Payments_07_PctPaidWithin30Days	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "%", "type": 0}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": true, "summaryTitle": "Payments_Group3_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Percentage"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_07_PctPaidWithin30Days_SummaryTitle	_Payments07_4	18
125	75a17266-ec29-4d15-a0b7-d6a42472f5c2	126	\N	16	1	t	Payments_07_PctPaid31To60Days	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "%", "type": 0}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": true, "summaryTitle": "Payments_Group3_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Percentage"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_07_PctPaid31To60Days_SummaryTitle	_Payments07_5	19
126	e39db484-561a-4002-843f-1249718a0453	127	\N	16	1	t	Payments_07_PctPaid61OrMoreDays	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "%", "type": 0}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": true, "summaryTitle": "Payments_Group3_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Percentage"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_07_PctPaid61OrMoreDays_SummaryTitle	_Payments07_6	20
127	4c934531-c9dc-455a-9c7c-156b3f6e36fd	128	\N	16	1	t	Payments_07_PctPaidOverdue	\N	{"layout": {"input": {"width": "2", "suffix": {"text": "%", "type": 0}}}, "grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": true, "summaryTitle": "Payments_Group3_SummaryTitle", "checkYourAnswers": true}, "validation": {"textValidationType": "Percentage"}}	2026-04-15 08:25:31.31733+00	2026-04-15 08:25:31.31733+00	\N	Payments_07_PctPaidOverdue_SummaryTitle	_Payments07_7	21
141	a53395f6-19cb-46b0-963a-e5b8faf67ebf	149	\N	17	1	t	CarbonNetZero_09_Title		{"layout": {"input": {"width": "3", "suffix": {"text": "CarbonNetZero_tCO2e", "type": 0}}}, "grouping": {"id": "75e44cb9-6cca-4f88-94d1-99cd4b8631ac", "page": true, "summaryTitle": "CarbonNetZero_BaselineEmissions_Group", "checkYourAnswers": true}, "validation": {"textValidationType": "Decimal"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	CarbonNetZero_09_SummaryTitle	_CarbonNetZeroQuestion09	9
143	8e86caa6-c03a-424e-9d7d-41da1d1f87c6	144	\N	17	1	t	CarbonNetZero_11_Title		{"layout": {"input": {"width": "3", "suffix": {"text": "CarbonNetZero_tCO2e", "type": 0}}}, "grouping": {"id": "e1df7ec0-79de-4ad9-aabf-e8d93304040b", "page": true, "summaryTitle": "CarbonNetZero_ReportingEmissions_Group", "checkYourAnswers": true}, "validation": {"textValidationType": "Decimal"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	CarbonNetZero_11_SummaryTitle	_CarbonNetZeroQuestion11	11
144	d167cf96-61c1-4047-b562-13f16ab18860	145	\N	17	1	t	CarbonNetZero_12_Title		{"layout": {"input": {"width": "3", "suffix": {"text": "CarbonNetZero_tCO2e", "type": 0}}}, "grouping": {"id": "e1df7ec0-79de-4ad9-aabf-e8d93304040b", "page": true, "summaryTitle": "CarbonNetZero_ReportingEmissions_Group", "checkYourAnswers": true}, "validation": {"textValidationType": "Decimal"}}	2026-04-15 08:25:31.357953+00	2026-04-15 08:25:31.357953+00	\N	CarbonNetZero_12_SummaryTitle	_CarbonNetZeroQuestion12	12
156	54c5ec69-521a-4d5c-b1e3-6b9721588390	\N	\N	18	6	t	Global_CheckYourAnswers		{}	2026-04-15 08:25:31.97341+00	2026-04-15 08:25:31.97341+00	\N	\N	_WelshExclusions06	6
178	bcff8d9f-25f5-4146-ad54-b6e46eac4c6b	179	\N	22	10	t	WelshHealthAndSafetyQuestion_03_Title	WelshHealthAndSafetyQuestion_03_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789001", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group1_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_03_SummaryTitle	_WelshHealthAndSafetyQuestion03	3
179	de4aa99a-efa4-459e-b92a-bfb404aa4f46	180	184	22	3	t	WelshHealthAndSafetyQuestion_04_Title	WelshHealthAndSafetyQuestion_04_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789002", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group2_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_04_SummaryTitle	_WelshHealthAndSafetyQuestion04	4
180	dd958587-3dfa-4021-a5c0-be151abc4071	181	\N	22	0	t	WelshHealthAndSafetyQuestion_05_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789003", "page": true, "summaryTitle": "WelshHealthAndSafetyQuestion_Group3_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N		_WelshHealthAndSafetyQuestion05	5
181	8d6f7e4d-68e1-44ab-bbe6-5990222f82ba	182	\N	22	1	t	WelshHealthAndSafetyQuestion_05_01_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789003", "page": true, "summaryTitle": "WelshHealthAndSafetyQuestion_Group3_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_05_01_SummaryTitle	_WelshHealthAndSafetyQuestion05_1	6
182	78c99e04-83f0-4446-a78b-9594f2e1aada	183	\N	22	1	t	WelshHealthAndSafetyQuestion_05_02_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789003", "page": true, "summaryTitle": "WelshHealthAndSafetyQuestion_Group3_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_05_02_SummaryTitle	_WelshHealthAndSafetyQuestion05_2	7
183	31f730d1-0a77-47e7-b18e-e511e7dcc0c9	184	\N	22	10	t	WelshHealthAndSafetyQuestion_05_03_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789003", "page": true, "summaryTitle": "WelshHealthAndSafetyQuestion_Group3_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_05_03_SummaryTitle	_WelshHealthAndSafetyQuestion05_3	8
185	80deff16-c954-4527-94c1-df7b38664349	186	\N	22	0	t	WelshHealthAndSafetyQuestion_07_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789005", "page": true, "summaryTitle": "WelshHealthAndSafetyQuestion_Group5_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N		_WelshHealthAndSafetyQuestion07	10
186	84aae73c-5553-4060-b20a-c9525952b99b	187	\N	22	1	t	WelshHealthAndSafetyQuestion_07_01_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789005", "page": true, "summaryTitle": "WelshHealthAndSafetyQuestion_Group5_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_07_01_SummaryTitle	_WelshHealthAndSafetyQuestion07_1	11
184	9bb68d4a-0ce3-44a6-8518-6bff9fca025e	185	194	22	3	t	WelshHealthAndSafetyQuestion_06_Title	WelshHealthAndSafetyQuestion_06_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789004", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group4_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_06_SummaryTitle	_WelshHealthAndSafetyQuestion06	9
177	602a9f1e-645b-4b29-8735-489e0dcade1e	178	179	22	3	t	WelshHealthAndSafetyQuestion_02_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789001", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group1_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_02_SummaryTitle	_WelshHealthAndSafetyQuestion02	2
193	cb3fc49d-03fd-4afb-a704-bc3e62189eb0	\N	\N	22	6	t	Global_CheckYourAnswers		{}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	\N	_WelshHealthAndSafetyQuestion12	21
187	ce5a1e08-837e-4df4-ad5b-c5cc5b4fa673	188	\N	22	1	t	WelshHealthAndSafetyQuestion_07_02_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789005", "page": true, "summaryTitle": "WelshHealthAndSafetyQuestion_Group5_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_07_02_SummaryTitle	_WelshHealthAndSafetyQuestion07_2	12
188	dbc47ead-6a7f-43a0-930e-bea8b080dd23	194	\N	22	10	t	WelshHealthAndSafetyQuestion_07_03_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789005", "page": true, "summaryTitle": "WelshHealthAndSafetyQuestion_Group5_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_07_03_SummaryTitle	_WelshHealthAndSafetyQuestion07_3	13
194	ba9b9b5f-84cb-42e7-bb25-3530408973ed	189	190	22	3	t	WelshHealthAndSafetyQuestion_08_01_Title	WelshHealthAndSafetyQuestion_08_01_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group6_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.395061+00	2026-04-15 08:25:32.395061+00	\N	WelshHealthAndSafetyQuestion_08_01_SummaryTitle	_WelshHealthAndSafetyQuestion08_1	14
195	38b40513-9700-4af1-b280-baf482cd9907	191	196	22	3	t	WelshHealthAndSafetyQuestion_10_01_Title	WelshHealthAndSafetyQuestion_10_01_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group7_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.395061+00	2026-04-15 08:25:32.395061+00	\N	WelshHealthAndSafetyQuestion_10_01_SummaryTitle	_WelshHealthAndSafetyQuestion10_1	17
196	4142c29e-bd2a-4429-894c-92bee71097e0	192	193	22	3	t	WelshHealthAndSafetyQuestion_11_01_Title	\N	{"layout": {"button": {"beforeButtonContent": "WelshHealthAndSafetyQuestion_11_01_CustomInsetText"}}, "grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group7_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.395061+00	2026-04-15 08:25:32.395061+00	\N	WelshHealthAndSafetyQuestion_11_01_SummaryTitle	_WelshHealthAndSafetyQuestion11_1	19
190	c28c43ef-d1a2-45c7-952a-88084c693d5d	195	\N	22	3	t	WelshHealthAndSafetyQuestion_09_Title	\N	{"grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group6_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_09_SummaryTitle	_WelshHealthAndSafetyQuestion09	16
189	de088ccf-297c-4309-9bf9-5383486328ab	190	\N	22	2	f	WelshHealthAndSafetyQuestion_08_Title	\N	{"layout": {"button": {"beforeButtonContent": "WelshHealthAndSafetyQuestion_08_CustomInsetText"}}, "grouping": {"id": "12345678-1234-1234-1234-123456789006", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group6_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_08_SummaryTitle	_WelshHealthAndSafetyQuestion08	15
191	d6f5313d-97f7-407d-bb18-a596d2504f6c	196	\N	22	2	f	WelshHealthAndSafetyQuestion_10_Title	\N	{"layout": {"button": {"beforeButtonContent": "WelshHealthAndSafetyQuestion_10_CustomInsetText"}}, "grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group7_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_10_SummaryTitle	_WelshHealthAndSafetyQuestion10	18
192	2e93d1e2-4b01-4587-bd2b-6ac0c46328ed	193	\N	22	2	f	WelshHealthAndSafetyQuestion_11_Title	\N	{"layout": {"button": {"beforeButtonContent": "WelshHealthAndSafetyQuestion_11_CustomInsetText"}}, "grouping": {"id": "12345678-1234-1234-1234-123456789007", "page": false, "summaryTitle": "WelshHealthAndSafetyQuestion_Group7_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:32.237371+00	2026-04-15 08:25:32.237371+00	\N	WelshHealthAndSafetyQuestion_11_SummaryTitle	_WelshHealthAndSafetyQuestion11	20
154	1aa0a3b3-3668-456e-8c34-3f4a3b916371	156	\N	18	10	t	WelshExclusions_04_Title	WelshExclusions_04_Description	{"grouping": {"id": "12345678-1234-1234-1234-123456789003", "page": false, "summaryTitle": "WelshExclusions_Group3_SummaryTitle", "checkYourAnswers": true}}	2026-04-15 08:25:31.97341+00	2026-04-15 08:25:31.97341+00	\N	WelshExclusions_04_SummaryTitle	_WelshExclusions04	4
197	f28ad7d8-5c49-40af-96d5-826891637afe	158	\N	19	2	f	QualityManagementQuestion_03_Title	\N	{"layout": {"button": {"beforeButtonContent": "QualityManagementQuestion_03_CustomInsetText"}}}	2026-04-15 08:25:32.555307+00	2026-04-15 08:25:32.555307+00	\N	QualityManagementQuestion_03_SummaryTitle	_QualityManagementQuestion03	3
\.


--
-- Data for Name: form_answers; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.form_answers (id, question_id, form_answer_set_id, bool_value, numeric_value, date_value, start_value, end_value, text_value, option_value, created_on, updated_on, guid, address_value, created_from, json_value) FROM stdin;
\.


--
-- Data for Name: identifiers; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.identifiers (id, identifier_id, scheme, legal_name, uri, "primary", created_on, updated_on, organisation_id) FROM stdin;
1	220C02F7ECD045	GB-PPON	ConnectedPersonsOrg 220C02F7ECD045 Ltd.	http://localhost:8082/organisations/13f6ff41-6ce3-4b7e-908d-d809a655cf6e	t	2026-04-15 12:24:24.671724+00	2026-04-15 12:24:24.671724+00	1
2	250808FBE50B4D	GB-PPON	ConnectedPersonsOrg 220C02F7ECD045 Ltd.	http://localhost:8082/organisations/13f6ff41-6ce3-4b7e-908d-d809a655cf6e	f	2026-04-15 12:24:24.671724+00	2026-04-15 12:24:24.671724+00	1
3	506CD59CCFBF48	GB-PPON	Jacobi Inc and Sons	http://localhost:8082/organisations/4344ef5e-ddcf-41d2-b5c7-4a5ca13df128	t	2026-04-15 12:27:18.284106+00	2026-04-15 12:27:18.284106+00	2
4	AA96410143914F	GB-COH		http://localhost:8082/organisations/4344ef5e-ddcf-41d2-b5c7-4a5ca13df128	f	2026-04-15 12:27:18.284106+00	2026-04-15 12:27:18.284106+00	2
5	F2F0C95883E747	GB-PPON	Lowe Inc and Sons	http://localhost:8082/organisations/829b0d39-9db3-4325-b506-81c325dc8936	t	2026-04-15 12:42:04.318545+00	2026-04-15 12:42:04.318545+00	3
6	6DEF6CFA48714D	GB-COH		http://localhost:8082/organisations/829b0d39-9db3-4325-b506-81c325dc8936	f	2026-04-15 12:42:04.318545+00	2026-04-15 12:42:04.318545+00	3
7	550694EAF8D74F	GB-PPON	Mante, Reichert and Bahringer	http://localhost:8082/organisations/d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5	t	2026-04-15 12:53:39.965838+00	2026-04-15 12:53:39.965838+00	4
8	02969354C89448	GB-COH		http://localhost:8082/organisations/d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5	f	2026-04-15 12:53:39.965838+00	2026-04-15 12:53:39.965838+00	4
9	4451604AA62E40	GB-PPON	Bailey, Hodkiewicz and Olson	http://localhost:8082/organisations/e73a5c46-b8e6-46f9-8fb9-59af6800f768	t	2026-04-15 12:59:15.237882+00	2026-04-15 12:59:15.237882+00	5
10	471756EE30F144	GB-COH		http://localhost:8082/organisations/e73a5c46-b8e6-46f9-8fb9-59af6800f768	f	2026-04-15 12:59:15.237882+00	2026-04-15 12:59:15.237882+00	5
11	6791F9F8BEA84B	GB-PPON	Kihn-Kunze	http://localhost:8082/organisations/a849f727-140e-4c6e-b4cb-7fdcbc2506d1	t	2026-04-15 13:04:51.790952+00	2026-04-15 13:04:51.790952+00	6
12	535FEE4A627E4A	GB-COH		http://localhost:8082/organisations/a849f727-140e-4c6e-b4cb-7fdcbc2506d1	f	2026-04-15 13:04:51.790952+00	2026-04-15 13:04:51.790952+00	6
13	255CA6E625984C	GB-PPON	Ankunding LLC	http://localhost:8082/organisations/2f844e27-18e3-42e7-a136-221e91c75753	t	2026-04-15 13:08:19.018298+00	2026-04-15 13:08:19.018298+00	7
14	A20575F1F94B48	GB-COH		http://localhost:8082/organisations/2f844e27-18e3-42e7-a136-221e91c75753	f	2026-04-15 13:08:19.018298+00	2026-04-15 13:08:19.018298+00	7
15	EAB4E218A0D743	GB-PPON	Macejkovic-Swaniawski	http://localhost:8082/organisations/57898244-28be-4dc8-a6a6-ddfb96052861	t	2026-04-15 13:13:04.88707+00	2026-04-15 13:13:04.88707+00	8
16	09F9D5B31C8A4A	GB-COH		http://localhost:8082/organisations/57898244-28be-4dc8-a6a6-ddfb96052861	f	2026-04-15 13:13:04.88707+00	2026-04-15 13:13:04.88707+00	8
17	BDBD8EFC475247	GB-PPON	Jewess-Lesch	http://localhost:8082/organisations/9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a	t	2026-04-15 13:15:30.06984+00	2026-04-15 13:15:30.06984+00	9
18	A65EAA026B9241	GB-COH		http://localhost:8082/organisations/9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a	f	2026-04-15 13:15:30.06984+00	2026-04-15 13:15:30.06984+00	9
19	F80BF8148C7349	GB-PPON	Hoppe, Mills and O'Keefe	http://localhost:8082/organisations/0db96436-e4f0-4bcc-8825-b828910bf0b3	t	2026-04-15 13:18:56.009106+00	2026-04-15 13:18:56.009106+00	10
20	FD1BB6DEEDE04B	GB-COH		http://localhost:8082/organisations/0db96436-e4f0-4bcc-8825-b828910bf0b3	f	2026-04-15 13:18:56.009106+00	2026-04-15 13:18:56.009106+00	10
21	757D8EFD213D43	GB-PPON	Langosh-O'Conner	http://localhost:8082/organisations/2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98	t	2026-04-15 13:26:54.138169+00	2026-04-15 13:26:54.138169+00	11
22	C2829984C06A48	GB-COH		http://localhost:8082/organisations/2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98	f	2026-04-15 13:26:54.138169+00	2026-04-15 13:26:54.138169+00	11
23	8B5E26F366D345	GB-PPON	Hegmann-Toy	http://localhost:8082/organisations/d37589cf-2ded-4c9d-b956-a237de7d8b36	t	2026-04-15 13:30:00.970274+00	2026-04-15 13:30:00.970274+00	12
24	C0339B7D7FC14E	GB-COH		http://localhost:8082/organisations/d37589cf-2ded-4c9d-b956-a237de7d8b36	f	2026-04-15 13:30:00.970274+00	2026-04-15 13:30:00.970274+00	12
25	61C5DD0F61DF45	GB-PPON	Wisozk, Deckow and Armstrong	http://localhost:8082/organisations/0032ec59-3a20-4854-a4ed-3681f1b5c79a	t	2026-04-15 13:32:11.253718+00	2026-04-15 13:32:11.253718+00	13
26	466595CC13474E	GB-COH		http://localhost:8082/organisations/0032ec59-3a20-4854-a4ed-3681f1b5c79a	f	2026-04-15 13:32:11.253718+00	2026-04-15 13:32:11.253718+00	13
27	98C719D31B2D4B	GB-PPON	Purdy, Collier and Cronin	http://localhost:8082/organisations/9502bf6a-3fca-4a7b-8a51-f74aca96e2db	t	2026-04-15 13:35:53.988093+00	2026-04-15 13:35:53.988093+00	14
28	620E043839424D	GB-COH		http://localhost:8082/organisations/9502bf6a-3fca-4a7b-8a51-f74aca96e2db	f	2026-04-15 13:35:53.988093+00	2026-04-15 13:35:53.988093+00	14
29	97ED2A0F67CD46	GB-PPON	Friesen, Heidenreich and Gibson	http://localhost:8082/organisations/e8429bd6-05ee-4872-ba6f-a5539e6e0a3b	t	2026-04-15 13:37:00.803533+00	2026-04-15 13:37:00.803533+00	15
30	38086433C69A43	GB-COH		http://localhost:8082/organisations/e8429bd6-05ee-4872-ba6f-a5539e6e0a3b	f	2026-04-15 13:37:00.803533+00	2026-04-15 13:37:00.803533+00	15
31	7753ED0082464F	GB-PPON	Purdy LLC	http://localhost:8082/organisations/587cdb5b-1ca3-4fc2-9e03-e740f95b37a5	t	2026-04-15 13:38:24.126018+00	2026-04-15 13:38:24.126018+00	16
32	BFA07CDD5DE04D	GB-COH		http://localhost:8082/organisations/587cdb5b-1ca3-4fc2-9e03-e740f95b37a5	f	2026-04-15 13:38:24.126018+00	2026-04-15 13:38:24.126018+00	16
33	A77E946F12D34D	GB-PPON	Gislason, Kris and Denesik	http://localhost:8082/organisations/6ee2465b-4664-41cd-a914-d53fe204af5e	t	2026-04-15 13:42:39.025353+00	2026-04-15 13:42:39.025353+00	17
34	9C2388F68F484C	GB-COH		http://localhost:8082/organisations/6ee2465b-4664-41cd-a914-d53fe204af5e	f	2026-04-15 13:42:39.025353+00	2026-04-15 13:42:39.025353+00	17
35	A6B4CB1CA6234A	GB-PPON	Ruecker-Bergnaum	http://localhost:8082/organisations/54ecfe58-b278-46cc-9c6f-c62295ef5505	t	2026-04-15 13:44:41.690217+00	2026-04-15 13:44:41.690217+00	18
36	B003591CEC3845	GB-COH		http://localhost:8082/organisations/54ecfe58-b278-46cc-9c6f-c62295ef5505	f	2026-04-15 13:44:41.690217+00	2026-04-15 13:44:41.690217+00	18
37	A6FD1D49D44442	GB-PPON	Considine, D'Amore and Hand	http://localhost:8082/organisations/d35f709c-9af4-4a25-815e-a01653bc5de3	t	2026-04-15 13:45:44.042358+00	2026-04-15 13:45:44.042358+00	19
38	F6B3DABAFAE844	GB-COH		http://localhost:8082/organisations/d35f709c-9af4-4a25-815e-a01653bc5de3	f	2026-04-15 13:45:44.042358+00	2026-04-15 13:45:44.042358+00	19
39	17BF5570601B43	GB-PPON	Cummings-Bailey	http://localhost:8082/organisations/19c08015-d446-45b4-92bf-5bbc45c06bce	t	2026-04-15 13:48:07.772765+00	2026-04-15 13:48:07.772765+00	20
40	6FB9BF27BDFF4B	GB-COH		http://localhost:8082/organisations/19c08015-d446-45b4-92bf-5bbc45c06bce	f	2026-04-15 13:48:07.772765+00	2026-04-15 13:48:07.772765+00	20
\.


--
-- Data for Name: identifiers_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.identifiers_snapshot (id, shared_consent_id, identifier_id, scheme, legal_name, uri, "primary", created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: supplier_information; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.supplier_information (id, supplier_type, operation_types, completed_reg_address, completed_postal_address, completed_vat, completed_website_address, completed_email_address, completed_operation_type, completed_legal_form, created_on, updated_on, completed_connected_person) FROM stdin;
\.


--
-- Data for Name: legal_forms; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.legal_forms (id, registered_under_act2006, registered_legal_form, law_registered, registration_date, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: supplier_information_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.supplier_information_snapshot (id, shared_consent_id, supplier_type, operation_types, completed_reg_address, completed_postal_address, completed_vat, completed_website_address, completed_email_address, completed_operation_type, completed_legal_form, completed_connected_person, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: legal_forms_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.legal_forms_snapshot (id, registered_under_act2006, registered_legal_form, law_registered, registration_date, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: mou; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.mou (id, guid, file_path, created_on, updated_on) FROM stdin;
1	1170db62-9657-4661-9b30-041b3fe234c2	version-1.pdf	2026-04-15 08:25:29.37536+00	2026-04-15 08:25:29.37536+00
\.


--
-- Data for Name: mou_email_reminders; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.mou_email_reminders (id, organisation_id, reminder_sent_on, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: mou_signature; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.mou_signature (id, signature_guid, organisation_id, created_by_id, job_title, mou_id, created_on, updated_on, name) FROM stdin;
1	ae53eff7-cddb-41e6-b7de-3b2a9f4af3fb	10	1	hhhh	1	2026-04-15 13:20:46.551095+00	2026-04-15 13:20:46.551095+00	hh
\.


--
-- Data for Name: organisation_address; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.organisation_address (id, type, address_id, organisation_id) FROM stdin;
1	1	1	1
2	1	2	2
3	1	3	3
4	1	4	4
5	1	5	5
6	1	6	6
7	1	7	7
8	1	8	8
9	1	9	9
10	1	10	10
11	1	11	11
12	1	12	12
13	1	13	13
14	1	14	14
15	1	15	15
16	1	16	16
17	1	17	17
18	1	18	18
19	1	19	19
20	1	20	20
\.


--
-- Data for Name: organisation_address_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.organisation_address_snapshot (id, shared_consent_id, type, address_id) FROM stdin;
\.


--
-- Data for Name: organisation_hierarchies; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.organisation_hierarchies (id, relationship_id, parent_organisation_id, child_organisation_id, created_on, superseded_on) FROM stdin;
\.


--
-- Data for Name: organisation_join_requests; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.organisation_join_requests (id, guid, organisation_id, person_id, status, reviewed_on, reviewed_by_id, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: organisation_parties; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.organisation_parties (id, parent_organisation_id, child_organisation_id, organisation_relationship, shared_consent_id, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: organisation_person; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.organisation_person (person_id, organisation_id, scopes, created_on, updated_on) FROM stdin;
1	1	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 12:24:24.671724+00	2026-04-15 12:24:24.671724+00
1	2	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 12:27:18.284106+00	2026-04-15 12:27:18.284106+00
1	3	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 12:42:04.318545+00	2026-04-15 12:42:04.318545+00
1	4	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 12:53:39.965838+00	2026-04-15 12:53:39.965838+00
1	5	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 12:59:15.237882+00	2026-04-15 12:59:15.237882+00
1	6	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:04:51.790952+00	2026-04-15 13:04:51.790952+00
1	7	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:08:19.018298+00	2026-04-15 13:08:19.018298+00
1	8	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:13:04.88707+00	2026-04-15 13:13:04.88707+00
1	9	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:15:30.06984+00	2026-04-15 13:15:30.06984+00
1	10	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:18:56.009106+00	2026-04-15 13:18:56.009106+00
1	11	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:26:54.138169+00	2026-04-15 13:26:54.138169+00
1	12	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:30:00.970274+00	2026-04-15 13:30:00.970274+00
1	13	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:32:11.253718+00	2026-04-15 13:32:11.253718+00
1	14	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:35:53.988093+00	2026-04-15 13:35:53.988093+00
1	15	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:37:00.803533+00	2026-04-15 13:37:00.803533+00
1	16	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:38:24.126018+00	2026-04-15 13:38:24.126018+00
1	17	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:42:39.025353+00	2026-04-15 13:42:39.025353+00
1	18	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:44:41.690217+00	2026-04-15 13:44:41.690217+00
1	19	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:45:44.042358+00	2026-04-15 13:45:44.042358+00
1	20	["ADMIN", "RESPONDER", "EDITOR"]	2026-04-15 13:48:07.772765+00	2026-04-15 13:48:07.772765+00
\.


--
-- Data for Name: organisations_snapshot; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.organisations_snapshot (id, shared_consent_id, name) FROM stdin;
\.


--
-- Data for Name: outbox_messages; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.outbox_messages (id, type, message, published, created_on, updated_on, queue_url, message_group_id) FROM stdin;
1	OrganisationRegistered	{"Id":"13f6ff41-6ce3-4b7e-908d-d809a655cf6e","Name":"ConnectedPersonsOrg 220C02F7ECD045","Identifier":{"Scheme":"GB-PPON","Id":"220C02F7ECD045","LegalName":"ConnectedPersonsOrg 220C02F7ECD045 Ltd.","Uri":"http://localhost:8082/organisations/13f6ff41-6ce3-4b7e-908d-d809a655cf6e"},"AdditionalIdentifiers":[{"Scheme":"GB-PPON","Id":"250808FBE50B4D","LegalName":"ConnectedPersonsOrg 220C02F7ECD045 Ltd.","Uri":"http://localhost:8082/organisations/13f6ff41-6ce3-4b7e-908d-d809a655cf6e"}],"Addresses":[{"StreetAddress":"123 Test Street","Locality":"London","Region":null,"PostalCode":"DA11 8HJ","CountryName":"United Kingdom","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Test Contact","Email":"contact@test.com","Telephone":"079256123321","Url":"https://test.com"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:24:24.799399+00	2026-04-15 12:24:25.660452+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
2	OrganisationRegistered	{"Id":"13f6ff41-6ce3-4b7e-908d-d809a655cf6e","Name":"ConnectedPersonsOrg 220C02F7ECD045","Identifier":{"Scheme":"GB-PPON","Id":"220C02F7ECD045","LegalName":"ConnectedPersonsOrg 220C02F7ECD045 Ltd.","Uri":"http://localhost:8082/organisations/13f6ff41-6ce3-4b7e-908d-d809a655cf6e"},"AdditionalIdentifiers":[{"Scheme":"GB-PPON","Id":"250808FBE50B4D","LegalName":"ConnectedPersonsOrg 220C02F7ECD045 Ltd.","Uri":"http://localhost:8082/organisations/13f6ff41-6ce3-4b7e-908d-d809a655cf6e"}],"Addresses":[{"StreetAddress":"123 Test Street","Locality":"London","Region":null,"PostalCode":"DA11 8HJ","CountryName":"United Kingdom","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Test Contact","Email":"contact@test.com","Telephone":"079256123321","Url":"https://test.com"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:24:24.817316+00	2026-04-15 12:24:25.729008+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
3	OrganisationRegistered	{"Id":"4344ef5e-ddcf-41d2-b5c7-4a5ca13df128","Name":"Heller LLC","Identifier":{"Scheme":"GB-PPON","Id":"506CD59CCFBF48","LegalName":"Jacobi Inc and Sons","Uri":"http://localhost:8082/organisations/4344ef5e-ddcf-41d2-b5c7-4a5ca13df128"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"AA96410143914F","LegalName":"","Uri":"http://localhost:8082/organisations/4344ef5e-ddcf-41d2-b5c7-4a5ca13df128"}],"Addresses":[{"StreetAddress":"41599 Lupe Hill","Locality":"North Kaleb","Region":"","PostalCode":"sj34 5sb","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Daron Hahn","Email":"kailee.ohara@smitham.info","Telephone":"856.723.9196","Url":"http://www.millerwintheiser.name/shop/food/page.jsp"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:27:18.306497+00	2026-04-15 12:27:18.36896+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
4	OrganisationRegistered	{"Id":"4344ef5e-ddcf-41d2-b5c7-4a5ca13df128","Name":"Heller LLC","Identifier":{"Scheme":"GB-PPON","Id":"506CD59CCFBF48","LegalName":"Jacobi Inc and Sons","Uri":"http://localhost:8082/organisations/4344ef5e-ddcf-41d2-b5c7-4a5ca13df128"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"AA96410143914F","LegalName":"","Uri":"http://localhost:8082/organisations/4344ef5e-ddcf-41d2-b5c7-4a5ca13df128"}],"Addresses":[{"StreetAddress":"41599 Lupe Hill","Locality":"North Kaleb","Region":"","PostalCode":"sj34 5sb","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Daron Hahn","Email":"kailee.ohara@smitham.info","Telephone":"856.723.9196","Url":"http://www.millerwintheiser.name/shop/food/page.jsp"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:27:18.309494+00	2026-04-15 12:27:18.376418+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
5	OrganisationRegistered	{"Id":"829b0d39-9db3-4325-b506-81c325dc8936","Name":"Pfeffer-Kohler","Identifier":{"Scheme":"GB-PPON","Id":"F2F0C95883E747","LegalName":"Lowe Inc and Sons","Uri":"http://localhost:8082/organisations/829b0d39-9db3-4325-b506-81c325dc8936"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"6DEF6CFA48714D","LegalName":"","Uri":"http://localhost:8082/organisations/829b0d39-9db3-4325-b506-81c325dc8936"}],"Addresses":[{"StreetAddress":"923 Dietrich Radial","Locality":"New Retha","Region":"","PostalCode":"cz81 7sd","CountryName":"Scotland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Mrs. Evelyn Bell Lebsack PhD","Email":"trinity@gottlieb.name","Telephone":"(819)571-0223","Url":"http://www.watsica.ca/interviews/form.gem"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:42:04.342825+00	2026-04-15 12:42:04.481949+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
6	OrganisationRegistered	{"Id":"829b0d39-9db3-4325-b506-81c325dc8936","Name":"Pfeffer-Kohler","Identifier":{"Scheme":"GB-PPON","Id":"F2F0C95883E747","LegalName":"Lowe Inc and Sons","Uri":"http://localhost:8082/organisations/829b0d39-9db3-4325-b506-81c325dc8936"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"6DEF6CFA48714D","LegalName":"","Uri":"http://localhost:8082/organisations/829b0d39-9db3-4325-b506-81c325dc8936"}],"Addresses":[{"StreetAddress":"923 Dietrich Radial","Locality":"New Retha","Region":"","PostalCode":"cz81 7sd","CountryName":"Scotland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Mrs. Evelyn Bell Lebsack PhD","Email":"trinity@gottlieb.name","Telephone":"(819)571-0223","Url":"http://www.watsica.ca/interviews/form.gem"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:42:04.347017+00	2026-04-15 12:42:04.499658+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
8	OrganisationRegistered	{"Id":"d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5","Name":"Zieme LLC","Identifier":{"Scheme":"GB-PPON","Id":"550694EAF8D74F","LegalName":"Mante, Reichert and Bahringer","Uri":"http://localhost:8082/organisations/d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"02969354C89448","LegalName":"","Uri":"http://localhost:8082/organisations/d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5"}],"Addresses":[{"StreetAddress":"89958 Mafalda Pike","Locality":"Terranceborough","Region":"","PostalCode":"fl2 8hr","CountryName":"Scotland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Shayna Mitchell","Email":"clovis_dickinson@douglas.biz","Telephone":"1-650-453-6197 x96149","Url":"http://www.williamson.com/catalog/form.gem"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:53:39.991936+00	2026-04-15 12:53:40.118868+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
7	OrganisationRegistered	{"Id":"d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5","Name":"Zieme LLC","Identifier":{"Scheme":"GB-PPON","Id":"550694EAF8D74F","LegalName":"Mante, Reichert and Bahringer","Uri":"http://localhost:8082/organisations/d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"02969354C89448","LegalName":"","Uri":"http://localhost:8082/organisations/d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5"}],"Addresses":[{"StreetAddress":"89958 Mafalda Pike","Locality":"Terranceborough","Region":"","PostalCode":"fl2 8hr","CountryName":"Scotland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Shayna Mitchell","Email":"clovis_dickinson@douglas.biz","Telephone":"1-650-453-6197 x96149","Url":"http://www.williamson.com/catalog/form.gem"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:53:39.98585+00	2026-04-15 12:53:40.098241+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
9	OrganisationRegistered	{"Id":"e73a5c46-b8e6-46f9-8fb9-59af6800f768","Name":"Schoen, Homenick and McLaughlin","Identifier":{"Scheme":"GB-PPON","Id":"4451604AA62E40","LegalName":"Bailey, Hodkiewicz and Olson","Uri":"http://localhost:8082/organisations/e73a5c46-b8e6-46f9-8fb9-59af6800f768"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"471756EE30F144","LegalName":"","Uri":"http://localhost:8082/organisations/e73a5c46-b8e6-46f9-8fb9-59af6800f768"}],"Addresses":[{"StreetAddress":"3050 McClure Course","Locality":"New Sidmouth","Region":"","PostalCode":"du19 3gv","CountryName":"England","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Maiya McDermott","Email":"dayna.shanahan@barrows.co.uk","Telephone":"359.528.9669","Url":"http://www.ernser.biz/reviews/applet.rsx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:59:15.249836+00	2026-04-15 12:59:15.315891+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
10	OrganisationRegistered	{"Id":"e73a5c46-b8e6-46f9-8fb9-59af6800f768","Name":"Schoen, Homenick and McLaughlin","Identifier":{"Scheme":"GB-PPON","Id":"4451604AA62E40","LegalName":"Bailey, Hodkiewicz and Olson","Uri":"http://localhost:8082/organisations/e73a5c46-b8e6-46f9-8fb9-59af6800f768"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"471756EE30F144","LegalName":"","Uri":"http://localhost:8082/organisations/e73a5c46-b8e6-46f9-8fb9-59af6800f768"}],"Addresses":[{"StreetAddress":"3050 McClure Course","Locality":"New Sidmouth","Region":"","PostalCode":"du19 3gv","CountryName":"England","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Maiya McDermott","Email":"dayna.shanahan@barrows.co.uk","Telephone":"359.528.9669","Url":"http://www.ernser.biz/reviews/applet.rsx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 12:59:15.252513+00	2026-04-15 12:59:15.325821+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
11	OrganisationRegistered	{"Id":"a849f727-140e-4c6e-b4cb-7fdcbc2506d1","Name":"White-Rodriguez","Identifier":{"Scheme":"GB-PPON","Id":"6791F9F8BEA84B","LegalName":"Kihn-Kunze","Uri":"http://localhost:8082/organisations/a849f727-140e-4c6e-b4cb-7fdcbc2506d1"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"535FEE4A627E4A","LegalName":"","Uri":"http://localhost:8082/organisations/a849f727-140e-4c6e-b4cb-7fdcbc2506d1"}],"Addresses":[{"StreetAddress":"936 Ivy Stravenue","Locality":"Parisianhaven","Region":"","PostalCode":"zx5 3ed","CountryName":"Wales","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Dewayne Volkman","Email":"celia@bauch.com","Telephone":"(294)232-2264 x17204","Url":"http://www.stark.info/interviews/page.asp"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:04:51.810236+00	2026-04-15 13:04:51.869079+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
12	OrganisationRegistered	{"Id":"a849f727-140e-4c6e-b4cb-7fdcbc2506d1","Name":"White-Rodriguez","Identifier":{"Scheme":"GB-PPON","Id":"6791F9F8BEA84B","LegalName":"Kihn-Kunze","Uri":"http://localhost:8082/organisations/a849f727-140e-4c6e-b4cb-7fdcbc2506d1"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"535FEE4A627E4A","LegalName":"","Uri":"http://localhost:8082/organisations/a849f727-140e-4c6e-b4cb-7fdcbc2506d1"}],"Addresses":[{"StreetAddress":"936 Ivy Stravenue","Locality":"Parisianhaven","Region":"","PostalCode":"zx5 3ed","CountryName":"Wales","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Dewayne Volkman","Email":"celia@bauch.com","Telephone":"(294)232-2264 x17204","Url":"http://www.stark.info/interviews/page.asp"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:04:51.813542+00	2026-04-15 13:04:51.879996+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
13	OrganisationRegistered	{"Id":"2f844e27-18e3-42e7-a136-221e91c75753","Name":"Block-Green","Identifier":{"Scheme":"GB-PPON","Id":"255CA6E625984C","LegalName":"Ankunding LLC","Uri":"http://localhost:8082/organisations/2f844e27-18e3-42e7-a136-221e91c75753"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"A20575F1F94B48","LegalName":"","Uri":"http://localhost:8082/organisations/2f844e27-18e3-42e7-a136-221e91c75753"}],"Addresses":[{"StreetAddress":"6930 Stan Burgs","Locality":"Rathborough","Region":"","PostalCode":"yh7 1wi","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Hardy Rippin","Email":"curt_ohara@wilderman.co.uk","Telephone":"199-854-2670 x2596","Url":"http://www.quigley.ca/shop/films/template.aspx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:08:19.033776+00	2026-04-15 13:08:19.117372+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
14	OrganisationRegistered	{"Id":"2f844e27-18e3-42e7-a136-221e91c75753","Name":"Block-Green","Identifier":{"Scheme":"GB-PPON","Id":"255CA6E625984C","LegalName":"Ankunding LLC","Uri":"http://localhost:8082/organisations/2f844e27-18e3-42e7-a136-221e91c75753"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"A20575F1F94B48","LegalName":"","Uri":"http://localhost:8082/organisations/2f844e27-18e3-42e7-a136-221e91c75753"}],"Addresses":[{"StreetAddress":"6930 Stan Burgs","Locality":"Rathborough","Region":"","PostalCode":"yh7 1wi","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Hardy Rippin","Email":"curt_ohara@wilderman.co.uk","Telephone":"199-854-2670 x2596","Url":"http://www.quigley.ca/shop/films/template.aspx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:08:19.039506+00	2026-04-15 13:08:19.125118+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
15	OrganisationRegistered	{"Id":"57898244-28be-4dc8-a6a6-ddfb96052861","Name":"Pollich, Homenick and Carroll","Identifier":{"Scheme":"GB-PPON","Id":"EAB4E218A0D743","LegalName":"Macejkovic-Swaniawski","Uri":"http://localhost:8082/organisations/57898244-28be-4dc8-a6a6-ddfb96052861"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"09F9D5B31C8A4A","LegalName":"","Uri":"http://localhost:8082/organisations/57898244-28be-4dc8-a6a6-ddfb96052861"}],"Addresses":[{"StreetAddress":"5440 Wyman Lakes","Locality":"Zulauffort","Region":"","PostalCode":"af4 1ns","CountryName":"Wales","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Mr. Ed Kane Schumm Sr.","Email":"soledad_goyette@spinkabraun.us","Telephone":"025.685.8535","Url":"http://www.yost.biz/home/template.html"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:13:04.910528+00	2026-04-15 13:13:04.974238+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
16	OrganisationRegistered	{"Id":"57898244-28be-4dc8-a6a6-ddfb96052861","Name":"Pollich, Homenick and Carroll","Identifier":{"Scheme":"GB-PPON","Id":"EAB4E218A0D743","LegalName":"Macejkovic-Swaniawski","Uri":"http://localhost:8082/organisations/57898244-28be-4dc8-a6a6-ddfb96052861"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"09F9D5B31C8A4A","LegalName":"","Uri":"http://localhost:8082/organisations/57898244-28be-4dc8-a6a6-ddfb96052861"}],"Addresses":[{"StreetAddress":"5440 Wyman Lakes","Locality":"Zulauffort","Region":"","PostalCode":"af4 1ns","CountryName":"Wales","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Mr. Ed Kane Schumm Sr.","Email":"soledad_goyette@spinkabraun.us","Telephone":"025.685.8535","Url":"http://www.yost.biz/home/template.html"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:13:04.914154+00	2026-04-15 13:13:04.983497+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
17	OrganisationRegistered	{"Id":"9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a","Name":"Lesch-Treutel","Identifier":{"Scheme":"GB-PPON","Id":"BDBD8EFC475247","LegalName":"Jewess-Lesch","Uri":"http://localhost:8082/organisations/9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"A65EAA026B9241","LegalName":"","Uri":"http://localhost:8082/organisations/9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a"}],"Addresses":[{"StreetAddress":"91956 Douglas Drives","Locality":"South Juanitaborough","Region":"","PostalCode":"yb28 1kt","CountryName":"England","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Christy Terence Fahey III","Email":"clarissa@turcottestehr.ca","Telephone":"1-883-508-5531 x417","Url":"http://www.windlerorn.uk/home/form.htm"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:15:30.094687+00	2026-04-15 13:15:30.177157+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
18	OrganisationRegistered	{"Id":"9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a","Name":"Lesch-Treutel","Identifier":{"Scheme":"GB-PPON","Id":"BDBD8EFC475247","LegalName":"Jewess-Lesch","Uri":"http://localhost:8082/organisations/9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"A65EAA026B9241","LegalName":"","Uri":"http://localhost:8082/organisations/9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a"}],"Addresses":[{"StreetAddress":"91956 Douglas Drives","Locality":"South Juanitaborough","Region":"","PostalCode":"yb28 1kt","CountryName":"England","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Christy Terence Fahey III","Email":"clarissa@turcottestehr.ca","Telephone":"1-883-508-5531 x417","Url":"http://www.windlerorn.uk/home/form.htm"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:15:30.098337+00	2026-04-15 13:15:30.18688+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
19	OrganisationRegistered	{"Id":"0db96436-e4f0-4bcc-8825-b828910bf0b3","Name":"Rohan, Treutel and Emard","Identifier":{"Scheme":"GB-PPON","Id":"F80BF8148C7349","LegalName":"Hoppe, Mills and O\\u0027Keefe","Uri":"http://localhost:8082/organisations/0db96436-e4f0-4bcc-8825-b828910bf0b3"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"FD1BB6DEEDE04B","LegalName":"","Uri":"http://localhost:8082/organisations/0db96436-e4f0-4bcc-8825-b828910bf0b3"}],"Addresses":[{"StreetAddress":"0411 Corbin Glens","Locality":"Mitchellmouth","Region":"","PostalCode":"il1 6cb","CountryName":"England","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Johathan Barrows","Email":"ana@reichel.biz","Telephone":"(580)473-3474 x45079","Url":"http://www.trantow.info/shop/food/root.res"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:18:56.024449+00	2026-04-15 13:18:56.110691+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
20	OrganisationRegistered	{"Id":"0db96436-e4f0-4bcc-8825-b828910bf0b3","Name":"Rohan, Treutel and Emard","Identifier":{"Scheme":"GB-PPON","Id":"F80BF8148C7349","LegalName":"Hoppe, Mills and O\\u0027Keefe","Uri":"http://localhost:8082/organisations/0db96436-e4f0-4bcc-8825-b828910bf0b3"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"FD1BB6DEEDE04B","LegalName":"","Uri":"http://localhost:8082/organisations/0db96436-e4f0-4bcc-8825-b828910bf0b3"}],"Addresses":[{"StreetAddress":"0411 Corbin Glens","Locality":"Mitchellmouth","Region":"","PostalCode":"il1 6cb","CountryName":"England","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Johathan Barrows","Email":"ana@reichel.biz","Telephone":"(580)473-3474 x45079","Url":"http://www.trantow.info/shop/food/root.res"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:18:56.026606+00	2026-04-15 13:18:56.151177+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
21	OrganisationRegistered	{"Id":"2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98","Name":"Powlowski-Rodriguez","Identifier":{"Scheme":"GB-PPON","Id":"757D8EFD213D43","LegalName":"Langosh-O\\u0027Conner","Uri":"http://localhost:8082/organisations/2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"C2829984C06A48","LegalName":"","Uri":"http://localhost:8082/organisations/2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98"}],"Addresses":[{"StreetAddress":"668 Moriah Springs","Locality":"Jaskolskifort","Region":"","PostalCode":"rx3 3gc","CountryName":"Wales","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Mr. Tanya Carey Mraz Jr.","Email":"halle@haneromaguera.info","Telephone":"(503)906-0657 x17127","Url":"http://www.mitchellhoeger.co.uk/interviews/template.htm"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:26:54.211434+00	2026-04-15 13:26:54.444078+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
22	OrganisationRegistered	{"Id":"2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98","Name":"Powlowski-Rodriguez","Identifier":{"Scheme":"GB-PPON","Id":"757D8EFD213D43","LegalName":"Langosh-O\\u0027Conner","Uri":"http://localhost:8082/organisations/2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"C2829984C06A48","LegalName":"","Uri":"http://localhost:8082/organisations/2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98"}],"Addresses":[{"StreetAddress":"668 Moriah Springs","Locality":"Jaskolskifort","Region":"","PostalCode":"rx3 3gc","CountryName":"Wales","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Mr. Tanya Carey Mraz Jr.","Email":"halle@haneromaguera.info","Telephone":"(503)906-0657 x17127","Url":"http://www.mitchellhoeger.co.uk/interviews/template.htm"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:26:54.220813+00	2026-04-15 13:26:54.459312+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
23	OrganisationRegistered	{"Id":"d37589cf-2ded-4c9d-b956-a237de7d8b36","Name":"Schuster-Little","Identifier":{"Scheme":"GB-PPON","Id":"8B5E26F366D345","LegalName":"Hegmann-Toy","Uri":"http://localhost:8082/organisations/d37589cf-2ded-4c9d-b956-a237de7d8b36"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"C0339B7D7FC14E","LegalName":"","Uri":"http://localhost:8082/organisations/d37589cf-2ded-4c9d-b956-a237de7d8b36"}],"Addresses":[{"StreetAddress":"3779 Bartell Coves","Locality":"New Burleytown","Region":"","PostalCode":"kn05 9kf","CountryName":"England","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Haleigh Williamson","Email":"orion@jakubowski.us","Telephone":"065.048.9397","Url":"http://www.armstrongoberbrunner.ca/guide/index.asp"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:30:00.990968+00	2026-04-15 13:30:01.054313+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
24	OrganisationRegistered	{"Id":"d37589cf-2ded-4c9d-b956-a237de7d8b36","Name":"Schuster-Little","Identifier":{"Scheme":"GB-PPON","Id":"8B5E26F366D345","LegalName":"Hegmann-Toy","Uri":"http://localhost:8082/organisations/d37589cf-2ded-4c9d-b956-a237de7d8b36"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"C0339B7D7FC14E","LegalName":"","Uri":"http://localhost:8082/organisations/d37589cf-2ded-4c9d-b956-a237de7d8b36"}],"Addresses":[{"StreetAddress":"3779 Bartell Coves","Locality":"New Burleytown","Region":"","PostalCode":"kn05 9kf","CountryName":"England","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Haleigh Williamson","Email":"orion@jakubowski.us","Telephone":"065.048.9397","Url":"http://www.armstrongoberbrunner.ca/guide/index.asp"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:30:00.993628+00	2026-04-15 13:30:01.073418+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
25	OrganisationRegistered	{"Id":"0032ec59-3a20-4854-a4ed-3681f1b5c79a","Name":"Stoltenberg, Wintheiser and Bartell","Identifier":{"Scheme":"GB-PPON","Id":"61C5DD0F61DF45","LegalName":"Wisozk, Deckow and Armstrong","Uri":"http://localhost:8082/organisations/0032ec59-3a20-4854-a4ed-3681f1b5c79a"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"466595CC13474E","LegalName":"","Uri":"http://localhost:8082/organisations/0032ec59-3a20-4854-a4ed-3681f1b5c79a"}],"Addresses":[{"StreetAddress":"08833 Mylene Walk","Locality":"Charleneton","Region":"","PostalCode":"ci6 1vb","CountryName":"Scotland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Donald Homenick","Email":"isabella.cole@mitchell.name","Telephone":"496-953-4666 x1430","Url":"http://www.collier.ca/home/template.jsp"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:32:11.270335+00	2026-04-15 13:32:11.337626+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
26	OrganisationRegistered	{"Id":"0032ec59-3a20-4854-a4ed-3681f1b5c79a","Name":"Stoltenberg, Wintheiser and Bartell","Identifier":{"Scheme":"GB-PPON","Id":"61C5DD0F61DF45","LegalName":"Wisozk, Deckow and Armstrong","Uri":"http://localhost:8082/organisations/0032ec59-3a20-4854-a4ed-3681f1b5c79a"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"466595CC13474E","LegalName":"","Uri":"http://localhost:8082/organisations/0032ec59-3a20-4854-a4ed-3681f1b5c79a"}],"Addresses":[{"StreetAddress":"08833 Mylene Walk","Locality":"Charleneton","Region":"","PostalCode":"ci6 1vb","CountryName":"Scotland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Donald Homenick","Email":"isabella.cole@mitchell.name","Telephone":"496-953-4666 x1430","Url":"http://www.collier.ca/home/template.jsp"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:32:11.273924+00	2026-04-15 13:32:11.348987+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
27	OrganisationRegistered	{"Id":"9502bf6a-3fca-4a7b-8a51-f74aca96e2db","Name":"Davis LLC","Identifier":{"Scheme":"GB-PPON","Id":"98C719D31B2D4B","LegalName":"Purdy, Collier and Cronin","Uri":"http://localhost:8082/organisations/9502bf6a-3fca-4a7b-8a51-f74aca96e2db"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"620E043839424D","LegalName":"","Uri":"http://localhost:8082/organisations/9502bf6a-3fca-4a7b-8a51-f74aca96e2db"}],"Addresses":[{"StreetAddress":"7680 Orland Ford","Locality":"Lake Gerardtown","Region":"","PostalCode":"fp56 6tl","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Hellen Beatty","Email":"marques@connelly.com","Telephone":"(220)802-4168","Url":"http://www.dickinson.ca/shop/films/resource.html"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:35:54.003959+00	2026-04-15 13:35:54.061163+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
28	OrganisationRegistered	{"Id":"9502bf6a-3fca-4a7b-8a51-f74aca96e2db","Name":"Davis LLC","Identifier":{"Scheme":"GB-PPON","Id":"98C719D31B2D4B","LegalName":"Purdy, Collier and Cronin","Uri":"http://localhost:8082/organisations/9502bf6a-3fca-4a7b-8a51-f74aca96e2db"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"620E043839424D","LegalName":"","Uri":"http://localhost:8082/organisations/9502bf6a-3fca-4a7b-8a51-f74aca96e2db"}],"Addresses":[{"StreetAddress":"7680 Orland Ford","Locality":"Lake Gerardtown","Region":"","PostalCode":"fp56 6tl","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Hellen Beatty","Email":"marques@connelly.com","Telephone":"(220)802-4168","Url":"http://www.dickinson.ca/shop/films/resource.html"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:35:54.007931+00	2026-04-15 13:35:54.078646+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
29	OrganisationRegistered	{"Id":"e8429bd6-05ee-4872-ba6f-a5539e6e0a3b","Name":"Lindgren, Will and Stroman","Identifier":{"Scheme":"GB-PPON","Id":"97ED2A0F67CD46","LegalName":"Friesen, Heidenreich and Gibson","Uri":"http://localhost:8082/organisations/e8429bd6-05ee-4872-ba6f-a5539e6e0a3b"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"38086433C69A43","LegalName":"","Uri":"http://localhost:8082/organisations/e8429bd6-05ee-4872-ba6f-a5539e6e0a3b"}],"Addresses":[{"StreetAddress":"9154 Oberbrunner Rapids","Locality":"Waelchitown","Region":"","PostalCode":"wl2 1ch","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Freddy Rogahn Sr.","Email":"ali_maggio@morissetteokuneva.uk","Telephone":"264.040.3318 x667","Url":"http://www.beier.com/home/page.htm"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:37:00.813543+00	2026-04-15 13:37:00.883189+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
30	OrganisationRegistered	{"Id":"e8429bd6-05ee-4872-ba6f-a5539e6e0a3b","Name":"Lindgren, Will and Stroman","Identifier":{"Scheme":"GB-PPON","Id":"97ED2A0F67CD46","LegalName":"Friesen, Heidenreich and Gibson","Uri":"http://localhost:8082/organisations/e8429bd6-05ee-4872-ba6f-a5539e6e0a3b"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"38086433C69A43","LegalName":"","Uri":"http://localhost:8082/organisations/e8429bd6-05ee-4872-ba6f-a5539e6e0a3b"}],"Addresses":[{"StreetAddress":"9154 Oberbrunner Rapids","Locality":"Waelchitown","Region":"","PostalCode":"wl2 1ch","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Freddy Rogahn Sr.","Email":"ali_maggio@morissetteokuneva.uk","Telephone":"264.040.3318 x667","Url":"http://www.beier.com/home/page.htm"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:37:00.818403+00	2026-04-15 13:37:00.894411+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
31	OrganisationRegistered	{"Id":"587cdb5b-1ca3-4fc2-9e03-e740f95b37a5","Name":"Kohler Inc and Sons","Identifier":{"Scheme":"GB-PPON","Id":"7753ED0082464F","LegalName":"Purdy LLC","Uri":"http://localhost:8082/organisations/587cdb5b-1ca3-4fc2-9e03-e740f95b37a5"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"BFA07CDD5DE04D","LegalName":"","Uri":"http://localhost:8082/organisations/587cdb5b-1ca3-4fc2-9e03-e740f95b37a5"}],"Addresses":[{"StreetAddress":"497 Herman Drives","Locality":"Prosaccoland","Region":"","PostalCode":"yf66 4pp","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Webster Schoen","Email":"guillermo.klocko@harvey.uk","Telephone":"(278)595-9712","Url":"http://www.flatley.ca/films/template.htm"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:38:24.163308+00	2026-04-15 13:38:24.32136+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
32	OrganisationRegistered	{"Id":"587cdb5b-1ca3-4fc2-9e03-e740f95b37a5","Name":"Kohler Inc and Sons","Identifier":{"Scheme":"GB-PPON","Id":"7753ED0082464F","LegalName":"Purdy LLC","Uri":"http://localhost:8082/organisations/587cdb5b-1ca3-4fc2-9e03-e740f95b37a5"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"BFA07CDD5DE04D","LegalName":"","Uri":"http://localhost:8082/organisations/587cdb5b-1ca3-4fc2-9e03-e740f95b37a5"}],"Addresses":[{"StreetAddress":"497 Herman Drives","Locality":"Prosaccoland","Region":"","PostalCode":"yf66 4pp","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Webster Schoen","Email":"guillermo.klocko@harvey.uk","Telephone":"(278)595-9712","Url":"http://www.flatley.ca/films/template.htm"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:38:24.169655+00	2026-04-15 13:38:24.332938+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
33	OrganisationRegistered	{"Id":"6ee2465b-4664-41cd-a914-d53fe204af5e","Name":"Marks Group","Identifier":{"Scheme":"GB-PPON","Id":"A77E946F12D34D","LegalName":"Gislason, Kris and Denesik","Uri":"http://localhost:8082/organisations/6ee2465b-4664-41cd-a914-d53fe204af5e"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"9C2388F68F484C","LegalName":"","Uri":"http://localhost:8082/organisations/6ee2465b-4664-41cd-a914-d53fe204af5e"}],"Addresses":[{"StreetAddress":"90604 Botsford Ways","Locality":"Kleinland","Region":"","PostalCode":"dk3 7nc","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Ezequiel Devante Leffler Jr.","Email":"larue_ziemann@legros.co.uk","Telephone":"256-458-3481","Url":"http://www.mclaughlin.name/shop/food/page.rsx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:42:39.071537+00	2026-04-15 13:42:39.234525+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
34	OrganisationRegistered	{"Id":"6ee2465b-4664-41cd-a914-d53fe204af5e","Name":"Marks Group","Identifier":{"Scheme":"GB-PPON","Id":"A77E946F12D34D","LegalName":"Gislason, Kris and Denesik","Uri":"http://localhost:8082/organisations/6ee2465b-4664-41cd-a914-d53fe204af5e"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"9C2388F68F484C","LegalName":"","Uri":"http://localhost:8082/organisations/6ee2465b-4664-41cd-a914-d53fe204af5e"}],"Addresses":[{"StreetAddress":"90604 Botsford Ways","Locality":"Kleinland","Region":"","PostalCode":"dk3 7nc","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Ezequiel Devante Leffler Jr.","Email":"larue_ziemann@legros.co.uk","Telephone":"256-458-3481","Url":"http://www.mclaughlin.name/shop/food/page.rsx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:42:39.074874+00	2026-04-15 13:42:39.276154+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
35	OrganisationRegistered	{"Id":"54ecfe58-b278-46cc-9c6f-c62295ef5505","Name":"Waters LLC","Identifier":{"Scheme":"GB-PPON","Id":"A6B4CB1CA6234A","LegalName":"Ruecker-Bergnaum","Uri":"http://localhost:8082/organisations/54ecfe58-b278-46cc-9c6f-c62295ef5505"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"B003591CEC3845","LegalName":"","Uri":"http://localhost:8082/organisations/54ecfe58-b278-46cc-9c6f-c62295ef5505"}],"Addresses":[{"StreetAddress":"5634 Colleen Plain","Locality":"Wardburgh","Region":"","PostalCode":"za55 2pb","CountryName":"Scotland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Ramona Keeling","Email":"armani@rowe.uk","Telephone":"284-236-5083 x90474","Url":"http://www.frami.com/shop/books/root.rsx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:44:41.71052+00	2026-04-15 13:44:41.763672+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
36	OrganisationRegistered	{"Id":"54ecfe58-b278-46cc-9c6f-c62295ef5505","Name":"Waters LLC","Identifier":{"Scheme":"GB-PPON","Id":"A6B4CB1CA6234A","LegalName":"Ruecker-Bergnaum","Uri":"http://localhost:8082/organisations/54ecfe58-b278-46cc-9c6f-c62295ef5505"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"B003591CEC3845","LegalName":"","Uri":"http://localhost:8082/organisations/54ecfe58-b278-46cc-9c6f-c62295ef5505"}],"Addresses":[{"StreetAddress":"5634 Colleen Plain","Locality":"Wardburgh","Region":"","PostalCode":"za55 2pb","CountryName":"Scotland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Ramona Keeling","Email":"armani@rowe.uk","Telephone":"284-236-5083 x90474","Url":"http://www.frami.com/shop/books/root.rsx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:44:41.721055+00	2026-04-15 13:44:41.776471+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
37	OrganisationRegistered	{"Id":"d35f709c-9af4-4a25-815e-a01653bc5de3","Name":"Stoltenberg Group","Identifier":{"Scheme":"GB-PPON","Id":"A6FD1D49D44442","LegalName":"Considine, D\\u0027Amore and Hand","Uri":"http://localhost:8082/organisations/d35f709c-9af4-4a25-815e-a01653bc5de3"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"F6B3DABAFAE844","LegalName":"","Uri":"http://localhost:8082/organisations/d35f709c-9af4-4a25-815e-a01653bc5de3"}],"Addresses":[{"StreetAddress":"8275 Langosh Crossroad","Locality":"Lake Haylee","Region":"","PostalCode":"tv7 5fx","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Kraig Wanda Weimann DDS","Email":"curt.dach@tremblay.name","Telephone":"1-170-022-4412 x37573","Url":"http://www.volkman.com/articles/template.aspx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:45:44.054002+00	2026-04-15 13:45:44.120581+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
38	OrganisationRegistered	{"Id":"d35f709c-9af4-4a25-815e-a01653bc5de3","Name":"Stoltenberg Group","Identifier":{"Scheme":"GB-PPON","Id":"A6FD1D49D44442","LegalName":"Considine, D\\u0027Amore and Hand","Uri":"http://localhost:8082/organisations/d35f709c-9af4-4a25-815e-a01653bc5de3"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"F6B3DABAFAE844","LegalName":"","Uri":"http://localhost:8082/organisations/d35f709c-9af4-4a25-815e-a01653bc5de3"}],"Addresses":[{"StreetAddress":"8275 Langosh Crossroad","Locality":"Lake Haylee","Region":"","PostalCode":"tv7 5fx","CountryName":"Northern Ireland","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Kraig Wanda Weimann DDS","Email":"curt.dach@tremblay.name","Telephone":"1-170-022-4412 x37573","Url":"http://www.volkman.com/articles/template.aspx"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:45:44.057316+00	2026-04-15 13:45:44.129038+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
39	OrganisationRegistered	{"Id":"19c08015-d446-45b4-92bf-5bbc45c06bce","Name":"Raynor-Gleichner","Identifier":{"Scheme":"GB-PPON","Id":"17BF5570601B43","LegalName":"Cummings-Bailey","Uri":"http://localhost:8082/organisations/19c08015-d446-45b4-92bf-5bbc45c06bce"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"6FB9BF27BDFF4B","LegalName":"","Uri":"http://localhost:8082/organisations/19c08015-d446-45b4-92bf-5bbc45c06bce"}],"Addresses":[{"StreetAddress":"23519 Wisoky Junction","Locality":"North Vaughnburgh","Region":"","PostalCode":"zo7 0oq","CountryName":"Wales","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Garnet Miller","Email":"velma@harber.us","Telephone":"929-261-5660","Url":"http://www.hills.uk/reviews/form.html"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:48:07.788219+00	2026-04-15 13:48:07.90634+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/entity-verification.fifo	EntityVerification
40	OrganisationRegistered	{"Id":"19c08015-d446-45b4-92bf-5bbc45c06bce","Name":"Raynor-Gleichner","Identifier":{"Scheme":"GB-PPON","Id":"17BF5570601B43","LegalName":"Cummings-Bailey","Uri":"http://localhost:8082/organisations/19c08015-d446-45b4-92bf-5bbc45c06bce"},"AdditionalIdentifiers":[{"Scheme":"GB-COH","Id":"6FB9BF27BDFF4B","LegalName":"","Uri":"http://localhost:8082/organisations/19c08015-d446-45b4-92bf-5bbc45c06bce"}],"Addresses":[{"StreetAddress":"23519 Wisoky Junction","Locality":"North Vaughnburgh","Region":"","PostalCode":"zo7 0oq","CountryName":"Wales","Country":"GB","Type":"Registered"}],"ContactPoint":{"Name":"Garnet Miller","Email":"velma@harber.us","Telephone":"929-261-5660","Url":"http://www.hills.uk/reviews/form.html"},"Roles":[],"Type":"organisation"}	t	2026-04-15 13:48:07.790547+00	2026-04-15 13:48:07.926078+00	http://sqs.us-east-1.localhost.localstack.cloud:4566/000000000000/user-management.fifo	UserManagement
\.


--
-- Data for Name: person_invites; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.person_invites (id, guid, first_name, last_name, email, organisation_id, person_id, scopes, invite_sent_on, created_on, updated_on, expires_on) FROM stdin;
\.


--
-- Data for Name: refresh_tokens; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.refresh_tokens (id, token_hash, expiry_date, revoked, created_on, updated_on) FROM stdin;
1	53aixloD5zVR6sNpMo4cpKrw58865fECSYZ3T93PX9M=	2026-04-16 08:29:41.513338+00	\N	2026-04-15 08:29:41.541446+00	2026-04-15 08:29:41.541446+00
2	rQnDF+5nfAu7CzAVW2Mpmfa+9xwZ9HaPQ18G96T2dfE=	2026-04-16 12:12:12.856233+00	\N	2026-04-15 12:12:12.877652+00	2026-04-15 12:12:12.877652+00
3	YGOj+PH5j6iSMXNzN9G3s+7sU6177WaQ7yewOiO3f00=	2026-04-16 12:13:04.253999+00	\N	2026-04-15 12:13:04.255231+00	2026-04-15 12:13:04.255231+00
4	fL6iax+a2tIFotP0Ewe5+UcHJVRiZKFUGjBxaiszq78=	2026-04-16 12:21:56.57356+00	\N	2026-04-15 12:21:56.577062+00	2026-04-15 12:21:56.577062+00
5	uKHXUOUvUOCr8awEnXC+cRbdo/hJJg43MAiWGNZY6W8=	2026-04-16 12:22:24.999489+00	\N	2026-04-15 12:22:25.002529+00	2026-04-15 12:22:25.002529+00
6	zbbQcKrN6aGdSZL8LDsCfO/ls4P8dyB7J11xjKUYoT4=	2026-04-16 12:24:07.774109+00	\N	2026-04-15 12:24:07.777094+00	2026-04-15 12:24:07.777094+00
7	T7sSpUt5cbcf8e5kcrKU6Ge17Hw6gXNjUCAJ6AUwTuU=	2026-04-16 12:24:23.134531+00	\N	2026-04-15 12:24:23.135808+00	2026-04-15 12:24:23.135808+00
8	hrPgkOHBgE7Gy+bAvYSSaRQ8lCrnTs6BIfRavVb3Pdo=	2026-04-16 12:24:41.81822+00	\N	2026-04-15 12:24:41.819442+00	2026-04-15 12:24:41.819442+00
9	up0Y9VpiMG5KgcGQw6zqc/WSFX9Jj16Mmme4nivE14M=	2026-04-16 12:27:03.179348+00	\N	2026-04-15 12:27:03.180523+00	2026-04-15 12:27:03.180523+00
10	l5G3ogDDyw69b+G39PK1LJFEg6ARWREGPKGYm+MqL3k=	2026-04-16 12:41:51.710302+00	\N	2026-04-15 12:41:51.714309+00	2026-04-15 12:41:51.714309+00
11	W96uLTdzm9ttkf8TXlV753c8XKqABTYyTUCAoy1NPG0=	2026-04-16 12:53:32.149941+00	\N	2026-04-15 12:53:32.151607+00	2026-04-15 12:53:32.151607+00
12	4MWAwyDz5ZABhDgUsbapYlac3NUmemJoGoaVaFOAJi8=	2026-04-16 12:59:13.688328+00	\N	2026-04-15 12:59:13.690628+00	2026-04-15 12:59:13.690628+00
13	bRK/d7CRDdCNtNO07u6GtGnppquJC8duGGCpY3Hr9HY=	2026-04-16 13:04:50.424082+00	\N	2026-04-15 13:04:50.427557+00	2026-04-15 13:04:50.427557+00
15	8KLYnBpG3VEff0lUVsfDsfBU5/CXPz79/xyLt3PLw2Q=	2026-04-16 13:08:17.630665+00	\N	2026-04-15 13:08:17.632763+00	2026-04-15 13:08:17.632763+00
14	Ka0+zM6gXAbIGcGBVXzS7k4W0Jx3FjyjV8XtHWua3Gk=	2026-04-16 13:07:38.617628+00	t	2026-04-15 13:07:38.61943+00	2026-04-15 13:09:19.364199+00
16	yxWgxoQrvYJ17R/iSUEYgmacE7nZj/Agra1FqoWnmno=	2026-04-16 13:10:25.597858+00	\N	2026-04-15 13:10:25.59929+00	2026-04-15 13:10:25.59929+00
17	SQOfz9D54OXluA0AT6+xKU/g0yUGe0QI/j8V75Jf+vU=	2026-04-16 13:13:03.759355+00	\N	2026-04-15 13:13:03.76174+00	2026-04-15 13:13:03.76174+00
18	a0oIjaIohQbrwUG3RU1O3PrkPXnZ/yXpj8KqTEmAQv0=	2026-04-16 13:15:28.797038+00	\N	2026-04-15 13:15:28.79963+00	2026-04-15 13:15:28.79963+00
19	IQZnP7Zq5MBpYcW2HpqhPxe5BOqooAovWiRw6xzW1jg=	2026-04-16 13:18:55.033705+00	\N	2026-04-15 13:18:55.035731+00	2026-04-15 13:18:55.035731+00
20	F4zbIj53QI0UuXSY10RnlEqwEojI7mGz5907iggLnD4=	2026-04-16 13:19:27.511254+00	\N	2026-04-15 13:19:27.512737+00	2026-04-15 13:19:27.512737+00
21	9vOvNc+9rGPlNVxXuk+UpdxrzpkcVEMNHdSEbTYOXbI=	2026-04-16 13:19:46.70896+00	\N	2026-04-15 13:19:46.71013+00	2026-04-15 13:19:46.71013+00
22	K+GU2R4b/j9587EqvWusZsmJo/oCztYl6Wh025S7k/c=	2026-04-16 13:20:51.933088+00	\N	2026-04-15 13:20:51.934199+00	2026-04-15 13:20:51.934199+00
23	AzhnGwl9F2iaH6k7u7MI06Uzp/ffrsD2JoNdW+PzxLM=	2026-04-16 13:21:02.029373+00	\N	2026-04-15 13:21:02.03109+00	2026-04-15 13:21:02.03109+00
24	O/6DwogmOBFm84RxTMQmfKNonpTW1nVDt58TJ9zjbN8=	2026-04-16 13:25:36.941852+00	\N	2026-04-15 13:25:36.946162+00	2026-04-15 13:25:36.946162+00
25	y4ogYjPfeVlxFrUH6jOZu+ravSLxsv+rQ38OA4ktlQg=	2026-04-16 13:26:52.832634+00	\N	2026-04-15 13:26:52.839169+00	2026-04-15 13:26:52.839169+00
26	rO2XdQzwAzN0gBqUxI5f8n2pSNQ4QIZw28LRz6G/UTA=	2026-04-16 13:30:00.038645+00	\N	2026-04-15 13:30:00.040959+00	2026-04-15 13:30:00.040959+00
27	9O7JVY9qNMq3EUmm4gqnaJZ8ErJ9TzGXnMa7x2WJbi8=	2026-04-16 13:32:10.419358+00	\N	2026-04-15 13:32:10.420553+00	2026-04-15 13:32:10.420553+00
28	s2N+1N9b5GiD3zzEPmdYh1r99Te0gRiegLRJKZ0Vdaw=	2026-04-16 13:35:53.189205+00	\N	2026-04-15 13:35:53.193948+00	2026-04-15 13:35:53.193948+00
29	p3syvzsUHz8ePo9pF1C/Q1oTEiJQxd6PavonfPhuMs0=	2026-04-16 13:36:24.26985+00	\N	2026-04-15 13:36:24.272292+00	2026-04-15 13:36:24.272292+00
30	QAnFFRIdOx346CLcsWiTDIKFhO3taJGFtLA5sT1hYa4=	2026-04-16 13:36:59.96437+00	\N	2026-04-15 13:36:59.966287+00	2026-04-15 13:36:59.966287+00
31	0PBo+fzDJ1c6xbpJgZr2eI9/hugzklJBYrGmHMUvAWM=	2026-04-16 13:38:23.232945+00	\N	2026-04-15 13:38:23.243521+00	2026-04-15 13:38:23.243521+00
32	yqmdftNitzpz80q0dvjRXURlPqOg2QptB/+NN3dXnX4=	2026-04-16 13:42:06.98229+00	\N	2026-04-15 13:42:06.984087+00	2026-04-15 13:42:06.984087+00
33	cqCG8hLnJV1DEVJAlUxCCOPX0e4PFD/cnooxInw51Hc=	2026-04-16 13:42:38.165539+00	\N	2026-04-15 13:42:38.167169+00	2026-04-15 13:42:38.167169+00
34	lOmrc4KTErwwrZDzjRa54EK27VZrefM8XxyQAjgkFi8=	2026-04-16 13:43:41.928495+00	\N	2026-04-15 13:43:41.929854+00	2026-04-15 13:43:41.929854+00
35	WJ6DafTe5OsmOTqBQQ9kqrCEBZNRWs/I4ckHXmO6I+E=	2026-04-16 13:44:40.948885+00	\N	2026-04-15 13:44:40.950724+00	2026-04-15 13:44:40.950724+00
36	9s+ynBQ1aE/sGib+vsMkK3TylJxl+mODvrDRoPfGBvI=	2026-04-16 13:45:20.102815+00	\N	2026-04-15 13:45:20.104256+00	2026-04-15 13:45:20.104256+00
37	9MNRKuYRh+QUupppstivqD5fiN2eEnESQxRveHeVPG8=	2026-04-16 13:45:43.263327+00	\N	2026-04-15 13:45:43.264673+00	2026-04-15 13:45:43.264673+00
38	y3Z30UgSfU3wkwcyxp9PujYKmAaIOz667St3EVuaBlk=	2026-04-16 13:46:19.451352+00	\N	2026-04-15 13:46:19.453471+00	2026-04-15 13:46:19.453471+00
39	M7RkpFkvrBAgskZi69PjkacHZsDTXYOOl1Jt5Tq9Bho=	2026-04-16 13:48:06.73551+00	\N	2026-04-15 13:48:06.737665+00	2026-04-15 13:48:06.737665+00
40	iWNaj/AiqOirwWsC/1icprPzFzyJ4Ecy3TRE9xvBx7Q=	2026-04-16 13:49:17.646833+00	\N	2026-04-15 13:49:17.649015+00	2026-04-15 13:49:17.649015+00
41	5cAQofNfNs3X3p8LgebRhWgwkiq/a/+iC7yxrn2hrcw=	2026-04-16 13:51:59.755082+00	\N	2026-04-15 13:51:59.763285+00	2026-04-15 13:51:59.763285+00
42	qH2pUl5vhpmaJk1+j46AjU7OY28+3B+nruc0lebztJo=	2026-04-16 14:21:27.203025+00	\N	2026-04-15 14:21:27.208283+00	2026-04-15 14:21:27.208283+00
44	BS4P7e3/TQH4QJDXZANxy97U8bbfDfR5xeM80Dac6RA=	2026-04-16 14:50:26.718613+00	\N	2026-04-15 14:50:26.720326+00	2026-04-15 14:50:26.720326+00
43	pLGsi4XmSFBtGttDFzU+bS8xfQ3GaV1t0V4DhWirpAg=	2026-04-16 14:21:40.813543+00	t	2026-04-15 14:21:40.814494+00	2026-04-15 14:51:51.440497+00
45	OOw/xkwyVaEbTzIlnkCZ8nS8NIglHE09NHH7dWNMSzA=	2026-04-16 14:51:51.494108+00	\N	2026-04-15 14:51:51.509549+00	2026-04-15 14:51:51.509549+00
46	uNEB6RdYzKpNJ+NPXV169NjsAE/wEpHjo38nv17oxY0=	2026-04-16 15:19:40.969199+00	\N	2026-04-15 15:19:40.970987+00	2026-04-15 15:19:40.970987+00
\.


--
-- Data for Name: shared_consent_consortiums; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.shared_consent_consortiums (id, parent_shared_consent_id, child_shared_consent_id, created_on, updated_on) FROM stdin;
\.


--
-- Data for Name: tenant_person; Type: TABLE DATA; Schema: public; Owner: cdp_user
--

COPY public.tenant_person (tenant_id, person_id) FROM stdin;
1	1
2	1
3	1
4	1
5	1
6	1
7	1
8	1
9	1
10	1
11	1
12	1
13	1
14	1
15	1
16	1
17	1
18	1
19	1
20	1
\.


--
-- Data for Name: applications; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.applications (id, name, client_id, description, category, is_active, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by, guid, allows_multiple_role_assignments, is_enabled_by_default) FROM stdin;
6	Payments	payments	Manage payment information and approvals across procurement activities.	Procurement	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	d7e0c257-4f1b-4633-a183-d50659bf1f91	f	f
5	Financial Viability Risk Assessments	financial-viability-risk-assessments	Set up and manage financial viability assessments for suppliers, or complete and submit them in response to a contracting authority.	Procurement	t	f	\N	\N	2026-04-15 08:25:25.943591+00	migration:add-procurement-applications	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	039335c9-589c-4b82-90a7-5500c1d80e65	t	f
8	Find a Tender	find-a-tender	View and manage procurement opportunities, supplier information, and notices for public sector buying and bidding.	Procurement	t	f	\N	\N	2026-04-15 08:25:26.050843+00	migration:add-find-a-tender-default-access	\N	\N	e633697f-7d48-42fe-830d-06f187e09684	f	t
\.


--
-- Data for Name: application_permissions; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.application_permissions (id, application_id, name, description, is_active, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by) FROM stdin;
\.


--
-- Data for Name: application_roles; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.application_roles (id, application_id, name, description, is_active, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by, required_party_roles, organisation_information_scopes, sync_to_organisation_information) FROM stdin;
8	5	Assessor (external)	Can review submitted financial viability evidence for assigned suppliers as an external assessor and record assessment outcomes.	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	{1}	[]	f
9	5	Assessor (internal)	Can review submitted financial viability evidence for suppliers within your authority and record assessment outcomes.	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	{1}	[]	f
6	5	QA (external)	Can quality-assure completed assessments for assigned cases and provide external QA outcomes and feedback.	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	{1}	[]	f
7	5	QA (internal)	Can quality-assure completed assessments, request amendments, and sign off internal assessment recommendations.	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	{1}	[]	f
4	5	Author and Collaborator (external)	Can create, edit, and submit FVRA responses on behalf of your organisation as an external collaborator with delegated access.	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	{3}	[]	f
5	5	Author and Collaborator (internal)	Can create, edit, and submit FVRA responses and collaborate with internal colleagues on evidence.	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	{3}	[]	f
3	6	Admin	Full access to view and edit payment information on all pages.	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	{}	[]	f
1	6	Authoriser	Full access to authorise or approve payments.	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	{}	[]	f
2	6	Editor	Full access to edit existing payments.	t	f	\N	\N	2026-04-15 08:25:25.961345+00	migration:keep-payments-fvra	\N	\N	{}	[]	f
12	8	Editor (buyer)	Can create share codes; view, add, and edit organisation and supplier information; create API keys to share information with external procurement platforms and authorised integration partners; and manage notices.	t	f	\N	\N	2026-04-15 08:25:26.050843+00	migration:add-find-a-tender-default-access	\N	\N	{1}	["ADMIN", "RESPONDER"]	t
13	8	Editor (supplier)	Can create share codes, and view, add, and edit organisation and supplier information.	t	f	\N	\N	2026-04-15 08:25:26.050843+00	migration:add-find-a-tender-default-access	\N	\N	{3}	["EDITOR", "RESPONDER"]	t
10	8	Viewer (buyer)	Can view organisation information and supplier information, and manage notices.	t	f	\N	\N	2026-04-15 08:25:26.050843+00	migration:add-find-a-tender-default-access	\N	\N	{1}	["VIEWER", "RESPONDER"]	t
11	8	Viewer (supplier)	Can view organisation information and supplier information.	t	f	\N	\N	2026-04-15 08:25:26.050843+00	migration:add-find-a-tender-default-access	\N	\N	{3}	["VIEWER", "RESPONDER"]	t
\.


--
-- Data for Name: application_role_permissions; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.application_role_permissions (role_id, permission_id) FROM stdin;
\.


--
-- Data for Name: organisation_roles; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.organisation_roles (id, display_name, description, sync_to_organisation_information, auto_assign_default_applications, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by, organisation_information_scopes) FROM stdin;
1	Member	Can access assigned applications only. Cannot manage organisation settings or other users.	t	t	f	\N	\N	2026-04-15 08:25:26.096405+00	migration:consolidate-role-changes	\N	\N	["VIEWER"]
2	Admin	Can add and remove users, enable applications for users, and assign users to applications.	t	t	f	\N	\N	2026-04-15 08:25:26.096405+00	migration:consolidate-role-changes	\N	\N	["ADMIN"]
3	Owner	Full control of the organisation. An organisation must have at least one owner.	t	t	f	\N	\N	2026-04-15 08:25:26.096405+00	migration:consolidate-role-changes	\N	\N	["ADMIN"]
\.


--
-- Data for Name: organisations; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.organisations (id, cdp_organisation_guid, name, slug, is_active, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by) FROM stdin;
1	13f6ff41-6ce3-4b7e-908d-d809a655cf6e	ConnectedPersonsOrg 220C02F7ECD045	connectedpersonsorg-220c02f7ecd045	t	f	\N	\N	-infinity	system:org-sync	\N	\N
2	4344ef5e-ddcf-41d2-b5c7-4a5ca13df128	Heller LLC	heller-llc	t	f	\N	\N	-infinity	system:org-sync	\N	\N
3	829b0d39-9db3-4325-b506-81c325dc8936	Pfeffer-Kohler	pfeffer-kohler	t	f	\N	\N	-infinity	system:org-sync	\N	\N
4	d638ee5b-2fc2-4c3b-9bc2-f89f73f3bfd5	Zieme LLC	zieme-llc	t	f	\N	\N	-infinity	system:org-sync	\N	\N
5	e73a5c46-b8e6-46f9-8fb9-59af6800f768	Schoen, Homenick and McLaughlin	schoen-homenick-and-mclaughlin	t	f	\N	\N	-infinity	system:org-sync	\N	\N
6	a849f727-140e-4c6e-b4cb-7fdcbc2506d1	White-Rodriguez	white-rodriguez	t	f	\N	\N	-infinity	system:org-sync	\N	\N
7	2f844e27-18e3-42e7-a136-221e91c75753	Block-Green	block-green	t	f	\N	\N	-infinity	system:org-sync	\N	\N
8	57898244-28be-4dc8-a6a6-ddfb96052861	Pollich, Homenick and Carroll	pollich-homenick-and-carroll	t	f	\N	\N	-infinity	system:org-sync	\N	\N
9	9371fbbb-d7ed-4ba4-90a1-9df6084c8d8a	Lesch-Treutel	lesch-treutel	t	f	\N	\N	-infinity	system:org-sync	\N	\N
10	0db96436-e4f0-4bcc-8825-b828910bf0b3	Rohan, Treutel and Emard	rohan-treutel-and-emard	t	f	\N	\N	-infinity	system:org-sync	\N	\N
11	2cdf414d-43e4-4c0b-ba2b-5d0ed2b34b98	Powlowski-Rodriguez	powlowski-rodriguez	t	f	\N	\N	-infinity	system:org-sync	\N	\N
12	d37589cf-2ded-4c9d-b956-a237de7d8b36	Schuster-Little	schuster-little	t	f	\N	\N	-infinity	system:org-sync	\N	\N
13	0032ec59-3a20-4854-a4ed-3681f1b5c79a	Stoltenberg, Wintheiser and Bartell	stoltenberg-wintheiser-and-bartell	t	f	\N	\N	-infinity	system:org-sync	\N	\N
14	9502bf6a-3fca-4a7b-8a51-f74aca96e2db	Davis LLC	davis-llc	t	f	\N	\N	-infinity	system:org-sync	\N	\N
15	e8429bd6-05ee-4872-ba6f-a5539e6e0a3b	Lindgren, Will and Stroman	lindgren-will-and-stroman	t	f	\N	\N	-infinity	system:org-sync	\N	\N
16	587cdb5b-1ca3-4fc2-9e03-e740f95b37a5	Kohler Inc and Sons	kohler-inc-and-sons	t	f	\N	\N	-infinity	system:org-sync	\N	\N
17	6ee2465b-4664-41cd-a914-d53fe204af5e	Marks Group	marks-group	t	f	\N	\N	-infinity	system:org-sync	\N	\N
18	54ecfe58-b278-46cc-9c6f-c62295ef5505	Waters LLC	waters-llc	t	f	\N	\N	-infinity	system:org-sync	\N	\N
19	d35f709c-9af4-4a25-815e-a01653bc5de3	Stoltenberg Group	stoltenberg-group	t	f	\N	\N	-infinity	system:org-sync	\N	\N
20	19c08015-d446-45b4-92bf-5bbc45c06bce	Raynor-Gleichner	raynor-gleichner	t	f	\N	\N	-infinity	system:org-sync	\N	\N
\.


--
-- Data for Name: invite_role_mappings; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.invite_role_mappings (id, cdp_person_invite_guid, organisation_id, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by, organisation_role_id) FROM stdin;
\.


--
-- Data for Name: organisation_applications; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.organisation_applications (id, organisation_id, application_id, is_active, enabled_at, enabled_by, disabled_at, disabled_by, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by) FROM stdin;
\.


--
-- Data for Name: invite_role_application_assignments; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.invite_role_application_assignments (id, invite_role_mapping_id, organisation_application_id, application_role_id, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by) FROM stdin;
\.


--
-- Data for Name: user_organisation_memberships; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.user_organisation_memberships (id, user_principal_id, cdp_person_id, organisation_id, is_active, joined_at, invited_by, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by, organisation_role_id) FROM stdin;
\.


--
-- Data for Name: user_application_assignments; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.user_application_assignments (id, user_organisation_membership_id, organisation_application_id, is_active, assigned_at, assigned_by, revoked_at, revoked_by, is_deleted, deleted_at, deleted_by, created_at, created_by, modified_at, modified_by) FROM stdin;
\.


--
-- Data for Name: user_assignment_roles; Type: TABLE DATA; Schema: user_management; Owner: cdp_user
--

COPY user_management.user_assignment_roles (user_assignment_id, role_id) FROM stdin;
\.


--
-- Name: addresses_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.addresses_id_seq', 20, true);


--
-- Name: addresses_snapshot_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.addresses_snapshot_id_seq', 1, false);


--
-- Name: announcements_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.announcements_id_seq', 1, false);


--
-- Name: authentication_keys_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.authentication_keys_id_seq', 19, true);


--
-- Name: connected_entities_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.connected_entities_id_seq', 1, false);


--
-- Name: connected_entities_snapshot_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.connected_entities_snapshot_id_seq', 1, false);


--
-- Name: connected_entity_address_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.connected_entity_address_id_seq', 1, false);


--
-- Name: connected_entity_address_snapshot_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.connected_entity_address_snapshot_id_seq', 1, false);


--
-- Name: contact_points_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.contact_points_id_seq', 20, true);


--
-- Name: contact_points_snapshot_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.contact_points_snapshot_id_seq', 1, false);


--
-- Name: form_answer_sets_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.form_answer_sets_id_seq', 1, false);


--
-- Name: form_answers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.form_answers_id_seq', 1, false);


--
-- Name: form_questions_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.form_questions_id_seq', 197, true);


--
-- Name: form_sections_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.form_sections_id_seq', 22, true);


--
-- Name: forms_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.forms_id_seq', 6, true);


--
-- Name: identifiers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.identifiers_id_seq', 40, true);


--
-- Name: identifiers_snapshot_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.identifiers_snapshot_id_seq', 1, false);


--
-- Name: mou_email_reminders_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.mou_email_reminders_id_seq', 1, false);


--
-- Name: mou_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.mou_id_seq', 1, true);


--
-- Name: mou_signature_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.mou_signature_id_seq', 1, true);


--
-- Name: organisation_address_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.organisation_address_id_seq', 20, true);


--
-- Name: organisation_address_snapshot_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.organisation_address_snapshot_id_seq', 1, false);


--
-- Name: organisation_hierarchies_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.organisation_hierarchies_id_seq', 1, false);


--
-- Name: organisation_join_requests_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.organisation_join_requests_id_seq', 1, false);


--
-- Name: organisation_parties_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.organisation_parties_id_seq', 1, false);


--
-- Name: organisations_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.organisations_id_seq', 20, true);


--
-- Name: organisations_snapshot_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.organisations_snapshot_id_seq', 1, false);


--
-- Name: outbox_messages_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.outbox_messages_id_seq', 40, true);


--
-- Name: person_invites_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.person_invites_id_seq', 1, false);


--
-- Name: persons_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.persons_id_seq', 3, true);


--
-- Name: refresh_tokens_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.refresh_tokens_id_seq', 46, true);


--
-- Name: shared_consent_consortiums_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.shared_consent_consortiums_id_seq', 1, false);


--
-- Name: shared_consents_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.shared_consents_id_seq', 1, false);


--
-- Name: supplier_information_snapshot_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.supplier_information_snapshot_id_seq', 1, false);


--
-- Name: tenants_id_seq; Type: SEQUENCE SET; Schema: public; Owner: cdp_user
--

SELECT pg_catalog.setval('public.tenants_id_seq', 20, true);


--
-- Name: application_permissions_id_seq; Type: SEQUENCE SET; Schema: user_management; Owner: cdp_user
--

SELECT pg_catalog.setval('user_management.application_permissions_id_seq', 1, false);


--
-- Name: application_roles_id_seq; Type: SEQUENCE SET; Schema: user_management; Owner: cdp_user
--

SELECT pg_catalog.setval('user_management.application_roles_id_seq', 13, true);


--
-- Name: applications_id_seq; Type: SEQUENCE SET; Schema: user_management; Owner: cdp_user
--

SELECT pg_catalog.setval('user_management.applications_id_seq', 8, true);


--
-- Name: invite_role_application_assignments_id_seq; Type: SEQUENCE SET; Schema: user_management; Owner: cdp_user
--

SELECT pg_catalog.setval('user_management.invite_role_application_assignments_id_seq', 1, false);


--
-- Name: invite_role_mappings_id_seq; Type: SEQUENCE SET; Schema: user_management; Owner: cdp_user
--

SELECT pg_catalog.setval('user_management.invite_role_mappings_id_seq', 1, false);


--
-- Name: organisation_applications_id_seq; Type: SEQUENCE SET; Schema: user_management; Owner: cdp_user
--

SELECT pg_catalog.setval('user_management.organisation_applications_id_seq', 1, false);


--
-- Name: organisations_id_seq; Type: SEQUENCE SET; Schema: user_management; Owner: cdp_user
--

SELECT pg_catalog.setval('user_management.organisations_id_seq', 20, true);


--
-- Name: user_application_assignments_id_seq; Type: SEQUENCE SET; Schema: user_management; Owner: cdp_user
--

SELECT pg_catalog.setval('user_management.user_application_assignments_id_seq', 1, false);


--
-- Name: user_organisation_memberships_id_seq; Type: SEQUENCE SET; Schema: user_management; Owner: cdp_user
--

SELECT pg_catalog.setval('user_management.user_organisation_memberships_id_seq', 1, false);


--
-- PostgreSQL database dump complete
--

