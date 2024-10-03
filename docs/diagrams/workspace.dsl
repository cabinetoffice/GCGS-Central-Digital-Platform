workspace "Central Digital Platform" {

    !identifiers hierarchical

    model {
        supplier = person "Supplier"
        buyer = person "Buyer"
        eSender = softwareSystem "eSender" "Commercial Software that Buyers use to manage tender processes"
        oneLogin = softwareSystem "Gov.uk One Login" "Let users sign in and prove their identities to use your service"
        govUkNotify = softwareSystem "Gov.uk Notify" "Sends emails, text messages and letters"
        companiesHouse = softwareSystem "Companies House" "Provides registered company details"
        fts = softwareSystem "Find a Tender"
        cfs = softwareSystem "Supplier Information and Contracts Finder"
        ppg = softwareSystem "Public Procurement Gateway"
        cdp = softwareSystem "Central Digital Platform" "Supports procurement" {

            messageQueue = container "Message Queue" "" SQS "Message Queue" {
            }

            group "Entity Verification" {
                entityVerificationDatabase = container "Entity Verification Database" "" PostgreSQL Database {
                }
                entityVerification = container "Entity Verification" "" "Asp.Net Core" WebApi {
                    group "Libraries" {
                        !include "libraries/mq.dsl"
                    }
                    entityVerificationEndpoint = component "Entity Verification Endpoint" "Queries known identifers." "Asp.Net Core Web API"
                    useCase = component "Use Case"
                    persistence = component "Persistence" "EntityFramework Core model and repositories" "C# Namespace"
                    entityVerificationEndpoint -> useCase "Executes"
                    useCase -> persistence "Executes"
                    useCase -> mq "Calls"
                    mq -> messageQueue "publishes to / listens to" "HTTPS"
                    persistence -> entityVerificationDatabase "reads/writes" "SQL"
                }
            }

            group "Organisation Information" {
                organisationInformationDatabase = container "Organisation Information Database" "" PostgreSQL Database {
                }
                fileStorage = container "File Storage" "" S3 "File Storage" {
                }

                authority = container "Authority" "" "Asp.Net Core" WebApi {
                    group "Libraries" {
                        !include "libraries/organisationInformationPersistence.dsl"
                    }
                    openIdConfigurationEndpoint = component "OpenID Well-Known Configuration Endpoint" "Exposes OpenID configuration" "Asp.Net Core Web API"
                    openIdJwksConfigurationEndpoint = component "OpenID Well-Known JWKS Configuration Endpoint" "Exposes Json Web Key Set" "Asp.Net Core Web API"
                    tokenEndpoint = component "Token endpoint" "Exchanges a valid One Login token to a longer-lived token with additional claims." "Asp.Net Core Web API"
                    tokenService = component "Token Service" "Creates and validates authentication tokens"
                    tokenEndpoint -> tokenService "Calls"
                    tokenService -> organisationInformationPersisence "Calls"
                    organisationInformationPersisence -> organisationInformationDatabase "reads/writes" "SQL"
                }
                tenantApi = container "Tenant API" "" "Asp.Net Core" WebApi {
                    group "Libraries" {
                        !include "libraries/organisationInformationPersistence.dsl"
                    }
                    tenantEndpoint = component "Tenant Endpoint" "" "Asp.Net Core Web API"
                    useCase = component "Use Case"
                    tenantEndpoint -> useCase "Executes"
                    useCase -> organisationInformationPersisence "Calls"
                    organisationInformationPersisence -> organisationInformationDatabase "reads/writes" "SQL"
                }
                personApi = container "Person API" "" "Asp.Net Core" WebApi {
                    group "Libraries" {
                        !include "libraries/organisationInformationPersistence.dsl"
                    }
                    personEndpoint = component "Person Endpoint" "" "Asp.Net Core Web API"
                    useCase = component "Use Case"
                    personEndpoint -> useCase "Executes"
                    useCase -> organisationInformationPersisence "Calls"
                    organisationInformationPersisence -> organisationInformationDatabase "reads/writes" "SQL"
                }
                organisationApi = container "Organisation API" "" "Asp.Net Core" WebApi {
                    group "Libraries" {
                        !include "libraries/organisationInformationPersistence.dsl"
                        !include "libraries/mq.dsl"
                        !include "libraries/govUkNotify.dsl"
                    }
                    organisationEndpoint = component "Organisation Endpoint" "" "Asp.Net Core Web API"
                    useCase = component "Use Case"
                    organisationEndpoint -> useCase "Executes"
                    useCase -> organisationInformationPersisence "Calls"
                    useCase -> mq "Calls"
                    useCase -> govUkNotifyClient "Calls"
                    organisationInformationPersisence -> organisationInformationDatabase "reads/writes" "SQL"
                    mq -> messageQueue "publishes to / listens to" "HTTPS"
                    govUkNotifyClient -> govUkNotify "sends notifications with" "HTTPS"
                }
                formsApi = container "Forms API" "" "Asp.Net Core" WebApi {
                    group "Libraries" {
                        !include "libraries/organisationInformationPersistence.dsl"
                    }
                    formsEndpoint = component "Forms Endpoint" "" "Asp.Net Core Web API"
                    useCase = component "Use Case"
                    formsEndpoint -> useCase "Executes"
                    useCase -> fileStorage "writes to" "HTTPS"
                    useCase -> organisationInformationPersisence "Calls"
                    organisationInformationPersisence -> organisationInformationDatabase "reads/writes" "SQL"
                }
                dataSharingApi = container "Data Sharing API" "" "Asp.Net Core" WebApi {
                    group "Libraries" {
                        !include "libraries/organisationInformationPersistence.dsl"
                    }
                    dataSharingEndpoint = component "Data Sharing Endpoint" "" "Asp.Net Core Web API"
                    useCase = component "Use Case"
                    dataSharingEndpoint -> useCase "Executes"
                    useCase -> fileStorage "writes to / reads from" "HTTPS"
                    useCase -> organisationInformationPersisence "Calls"
                    organisationInformationPersisence -> organisationInformationDatabase "reads/writes" "SQL"
                }
                organisationApp = container "Organisation App" "Account & data capture frontend" "Asp.Net Core MVC" WebApp {
                    group "Libraries" {
                        !include "libraries/webApiClients.dsl"
                    }

                    mvcController = component "MVC Controller" "Enables web users to perform tasks." "Asp.Net Core MVC Controller"

                    supplier -> mvcController "Uses" "HTTPS"
                    buyer -> mvcController "Uses" "HTTPS"

                    mvcController -> oneLogin "Authenticates with" "HTTPS"
                    mvcController -> companiesHouse "Pulls company details from" "HTTPS"
                    mvcController -> authority.tokenEndpoint "Authenticates and authorises with" "HTTPS"
                    mvcController -> tenantClient "Uses"
                    mvcController -> personClient "Uses"
                    mvcController -> organisationClient "Uses"
                    mvcController -> formsClient "Uses"
                    mvcController -> dataSharingClient "Uses"
                    mvcController -> entityVerificationClient "Uses"
                    mvcController -> fileStorage "writes to" "HTTPS"

                    tenantClient -> tenantApi.tenantEndpoint "Calls" "HTTPS/json"
                    personClient -> personApi.personEndpoint "Calls" "HTTPS/json"
                    organisationClient -> organisationApi.organisationEndpoint "Calls" "HTTPS/json"
                    formsClient -> formsApi.formsEndpoint "Calls" "HTTPS/json"
                    dataSharingClient -> dataSharingApi.dataSharingEndpoint "Calls" "HTTPS/json"
                    entityVerificationClient -> entityVerification.entityVerificationEndpoint "Calls" "HTTPS/json"

                    -> cdp.authority.openIdConfigurationEndpoint "Retrieves OpenID configuration from"
                    -> cdp.authority.openIdJwksConfigurationEndpoint "Retrieves Json Web Key Set from"
                }
            }
        }

        eSender -> cdp.dataSharingApi.dataSharingEndpoint "Looks up supplier information" "HTTPS/json"
        buyer -> eSender "Uses"

        fts -> oneLogin "Authenticates with"
        fts -> cdp.authority.tokenEndpoint "Authenticates with"
        fts -> cdp.authority.openIdConfigurationEndpoint "Retrieves OpenID configuration from"
        fts -> cdp.authority.openIdJwksConfigurationEndpoint "Retrieves Json Web Key Set from"
        fts -> cdp.organisationApi.organisationEndpoint "Calls" "HTTPS/json"
        cfs -> cdp.authority "Authenticates with"
        ppg -> cdp.authority "Authenticates with"
    }

    views {
        systemContext cdp "CDP-1-SystemContext" {
            include *
            description "The system context diagram for the Central Digital Platform."
        }
        container cdp "CDP-2-ContainerView" {
            include *
            description "The container diagram for the Central Digital Platform."
        }
        component cdp.authority "CDP-3-Authority-Components" {
            include *
            description "The component diagram for the Authority service."
        }
        component cdp.organisationApp "CDP-3-OrganisationApp-Components" {
            include *
            description "The component diagram for the Web Application."
        }
        component cdp.tenantApi "CDP-4-TenantApi-Components" {
            include *
            description "The component diagram for the Tenant API."
        }
        component cdp.personApi "CDP-5-PersonApi-Components" {
            include *
            description "The component diagram for the Person API."
        }
        component cdp.organisationApi "CDP-6-OrganisationApi-Components" {
            include *
            description "The component diagram for the Organisation API."
        }
        component cdp.formsApi "CDP-7-FormsApi-Components" {
            include *
            description "The component diagram for the Forms API."
        }
        component cdp.dataSharingApi "CDP-8-DataSharingApi-Components" {
            include *
            description "The component diagram for the Data Sharing API."
        }
        component cdp.entityVerification "CDP-9-EntityVerification-Components" {
            include *
            description "The component diagram for the Entity Verification."
        }
        theme default
        styles {
            element Database {
                shape Cylinder
            }
            element "Message Queue" {
                shape Pipe
            }
            element "File Storage" {
                shape Folder
            }
            element WebApp {
                shape WebBrowser
            }
        }
    }

    configuration {
        scope softwaresystem
    }

}