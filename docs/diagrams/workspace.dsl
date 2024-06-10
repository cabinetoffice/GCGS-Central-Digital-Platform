workspace "Central Digital Platform" {

    model {
        supplier = person "Supplier"
        buyer = person "Buyer"
        eSender = softwareSystem "eSender" "Commercial Software that Buyers use to manage tender processes"
        oneLogin = softwareSystem "Gov.uk One Login" "Let users sign in and prove their identities to use your service"
        fts = softwareSystem "Find a Tender"
        cfs = softwareSystem "Supplier Information and Contracts Finder"
        ppg = softwareSystem "Public Procurement Gateway"
        cdp = softwareSystem "Central Digital Platform" "Supports procurement" {
            authority = container "Authority" "" "Asp.Net Core" WebApi {
                openIdConfigurationEndpoint = component "OpenID Well-Known Configuration Endpoint" "Exposes OpenID configuration" "Asp.Net Core Web API"
                openIdJwksConfigurationEndpoint = component "OpenID Well-Known JWKS Configuration Endpoint" "Exposes Json Web Key Set" "Asp.Net Core Web API"
                tokenEndpoint = component "Token endpoint" "Exchanges a valid One Login token to a longer-lived token with additional claims." "Asp.Net Core Web API"
            }
            tenantApi = container "Tenant API" "" "Asp.Net Core" WebApi {
                tenantEndpoint = component "Tenant Endpoint" "" "Asp.Net Core Web API"
                registerTenantUseCase = component "Register Tenant Use Case"
                getTenantUseCase = component "Get Tenant Use Case"
                tenantPersistence = component "Persistence" "" "Project"
                tenantEndpoint -> registerTenantUseCase "Executes"
                tenantEndpoint -> getTenantUseCase "Executes"
                registerTenantUseCase -> tenantPersistence "Calls"
                getTenantUseCase -> tenantPersistence "Calls"
            }
            personApi = container "Person API" "" "Asp.Net Core" WebApi {
                personEndpoint = component "Person Endpoint" "" "Asp.Net Core Web API"
            }
            organisationApi = container "Organisation API" "" "Asp.Net Core" WebApi {
                organisationEndpoint = component "Organisation Endpoint" "" "Asp.Net Core Web API"
            }
            formsApi = container "Forms API" "" "Asp.Net Core" WebApi {
                formsEndpoint = component "Forms Endpoint" "" "Asp.Net Core Web API"
            }
            dataSharingApi = container "Data Sharing API" "" "Asp.Net Core" WebApi {
                dataSharingEndpoint = component "Data Sharing Endpoint" "" "Asp.Net Core Web API"
            }
            database = container "Organisation Information Database" "" PostgreSQL Database {
                tenantPersistence -> database "reads/writes" "SQL"
            }
            webApp = container "Web Application" "Account & data capture frontend" "Asp.Net Core MVC" WebApp {
                mvcController = component "MVC Controller" "Enables web users to perform tasks." "Asp.Net Core MVC Controller"

                tenantClient = component "Tenant Client" "Makes API calls to the Tenant API" "library"
                personClient = component "Person Client" "Makes API calls to the Person API" "library"
                organisationClient = component "Organisation Client" "Makes API calls to the Organisation API" "library"
                formsClient = component "Forms Client" "Makes API calls to the Forms API" "library"
                dataSharingClient = component "Data Sharing Client" "Makes API calls to the Data Sharing API" "library"

                supplier -> mvcController "Uses" "HTTPS"

                mvcController -> oneLogin "Authenticates with" "HTTPS"
                mvcController -> tokenEndpoint "Authenticates and authorises with" "HTTPS"
                mvcController -> tenantClient "Uses"
                mvcController -> personClient "Uses"
                mvcController -> organisationClient "Uses"
                mvcController -> formsClient "Uses"
                mvcController -> dataSharingClient "Uses"
            }

            personApi -> database "reads/writes" "SQL"
            organisationApi -> database "reads/writes" "SQL"
            formsApi -> database "reads/writes" "SQL"
            dataSharingApi -> database "reads/writes" "SQL"

            tenantClient -> tenantEndpoint "Calls" "HTTPS/json"
            personClient -> personEndpoint "Calls" "HTTPS/json"
            organisationClient -> organisationEndpoint "Calls" "HTTPS/json"
            formsClient -> formsEndpoint "Calls" "HTTPS/json"
            dataSharingClient -> dataSharingEndpoint "Calls" "HTTPS/json"
            eSender -> dataSharingEndpoint "Looks up supplier information" "HTTPS/json"
        }

        buyer -> eSender "Uses"

        fts -> oneLogin "Authenticates with"
        fts -> tokenEndpoint "Authorizes with"
        fts -> openIdConfigurationEndpoint "Retrieves OpenID configuration from"
        fts -> openIdJwksConfigurationEndpoint "Retrieves Json Web Key Set from"
        webApp -> openIdConfigurationEndpoint "Retrieves OpenID configuration from"
        webApp -> openIdJwksConfigurationEndpoint "Retrieves Json Web Key Set from"
        cfs -> authority "Authorizes with"
        ppg -> authority "Authorizes with"
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
        component authority "CDP-3-Authority-Components" {
            include *
            description "The component diagram for the Authority service."
        }
        component webApp "CDP-3-WebApp-Components" {
            include *
            description "The component diagram for the Web Application."
        }
        component tenantApi "CDP-4-TenantApi-Components" {
            include *
            description "The component diagram for the Tenant API."
        }
        component personApi "CDP-5-PersonApi-Components" {
            include *
            description "The component diagram for the Person API."
        }
        component organisationApi "CDP-6-OrganisationApi-Components" {
            include *
            description "The component diagram for the Organisation API."
        }
        component formsApi "CDP-7-FormsApi-Components" {
            include *
            description "The component diagram for the Forms API."
        }
        component dataSharingApi "CDP-8-DataSharingApi-Components" {
            include *
            description "The component diagram for the Data Sharing API."
        }
        theme default
        styles {
            element Database {
                shape Cylinder
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