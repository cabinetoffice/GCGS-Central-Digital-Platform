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
            tenantApi = container "Tenant API" "" "Asp.Net Core" WebApi {
                tenantEndpoint = component "Tenant Endpoint" "" "Asp.Net Core Web API"
            }
            personApi = container "Person API" "" "Asp.Net Core" WebApi {
                personEndpoint = component "Person Endpoint" "" "Asp.Net Core Web API"
            }
            organisationApi = container "Organisation API" "" "Asp.Net Core" WebApi {
                organisationEndpoint = component "Organisation Endpoint" "" "Asp.Net Core Web API"
            }
            dataCaptureApi = container "Data Capture API" "" "Asp.Net Core" WebApi {
                dataCaptureEndpoint = component "Data Capture Endpoint" "" "Asp.Net Core Web API"
            }
            dataSharingApi = container "Data Sharing API" "" "Asp.Net Core" WebApi {
                dataSharingEndpoint = component "Data Sharing Endpoint" "" "Asp.Net Core Web API"
            }
            database = container "Organisation Information Database" "" PostgreSQL Database
            webApp = container "Web Application" "Account & data capture frontend" "Asp.Net Core MVC" WebApp {
                signInController = component "Sign In Controller" "Enables organisations to sign in to the Organisation Account." "Asp.Net Core MVC Controller"
                registrationController = component "Registration Controller" "Enables organisations to sign sign up for the Organisation Account." "Asp.Net Core MVC Controller"
                personController = component "Person Controller" "Enables organisations to manage organisation persons." "Asp.Net Core MVC Controller"
                coreDataController = component "Core Data Controller" "Enables organisations to provide core data for their Organisation." "Asp.Net Core MVC Controller"
                dataCaptureController = component "Data Capture Controller" "Enables organisations to provide additional details." "Asp.Net Core MVC Controller"
                dataSharingController = component "Data Sharing Controller" "Enables Suppliers to share their Organisation data with Buyers." "Asp.Net Core MVC Controller"

                tenantClient = component "Tenant Client" "Makes API calls to the Tenant API" "library"
                personClient = component "Person Client" "Makes API calls to the Person API" "library"
                organisationClient = component "Organisation Client" "Makes API calls to the Organisation API" "library"
                dataCaptureClient = component "Data Capture Client" "Makes API calls to the Data Capture API" "library"
                dataSharingClient = component "Data Sharing Client" "Makes API calls to the Data Sharing API" "library"

                supplier -> signInController "Uses" "HTTPS"
                supplier -> registrationController "Uses" "HTTPS"
                supplier -> personController "Uses" "HTTPS"
                supplier -> coreDataController "Uses" "HTTPS"
                supplier -> dataCaptureController "Uses" "HTTPS"
                supplier -> dataSharingController "Uses" "HTTPS"

                signInController -> oneLogin "Authenticates with" "HTTPS"
                registrationController -> tenantClient "Uses"
                registrationController -> personClient "Uses"
                registrationController -> organisationClient "Uses"
                personController -> personClient "Uses"
                coreDataController -> organisationClient "Uses"
                dataCaptureController -> dataCaptureClient "Uses"
                dataSharingController -> dataSharingClient "Uses"
            }

            tenantApi -> database "reads/writes" "SQL"
            personApi -> database "reads/writes" "SQL"
            organisationApi -> database "reads/writes" "SQL"
            dataCaptureApi -> database "reads/writes" "SQL"
            dataSharingApi -> database "reads/writes" "SQL"

            tenantClient -> tenantEndpoint "Calls" "HTTPS/json"
            personClient -> personEndpoint "Calls" "HTTPS/json"
            organisationClient -> organisationEndpoint "Calls" "HTTPS/json"
            dataCaptureClient -> dataCaptureEndpoint "Calls" "HTTPS/json"
            dataSharingClient -> dataSharingEndpoint "Calls" "HTTPS/json"
            eSender -> dataSharingEndpoint "Looks up supplier information" "HTTPS/json"
        }

        buyer -> eSender "Uses"

        fts -> cdp "Authorizes with"
        cfs -> cdp "Authorizes with"
        ppg -> cdp "Authorizes with"
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
        component dataCaptureApi "CDP-7-DataCaptureApi-Components" {
            include *
            description "The component diagram for the Data Capture API."
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