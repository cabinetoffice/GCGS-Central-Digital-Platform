workspace "Central Digital Platform" {

    model {
        supplier = person "Supplier"
        buyer = person "Buyer"
        eSender = softwareSystem "eSender" "Commercial Software that Buyers use to manage tender processes"
        cdp = softwareSystem "Central Digital Platform" "Supports procurement" {
            webApp = container "Web Application" "Account & data capture frontend" "Asp.Net Core MVC" WebApp
            tenantApi = container "Tenant API" "" "Asp.Net Core" WebApi
            personApi = container "Person API" "" "Asp.Net Core" WebApi
            organisationApi = container "Organisation API" "" "Asp.Net Core" WebApi
            dataCaptureApi = container "Data Capture API" "" "Asp.Net Core" WebApi
            dataSharingApi = container "Data Sharing API" "" "Asp.Net Core" WebApi
            database = container "Organisation Information Database" "" PostgreSQL Database

            webApp -> tenantApi "calls" "HTTPS/json"
            webApp -> personApi "calls" "HTTPS/json"
            webApp -> organisationApi "calls" "HTTPS/json"
            webApp -> dataCaptureApi "calls" "HTTPS/json"

            tenantApi -> database "reads/writes" "SQL"
            personApi -> database "reads/writes" "SQL"
            organisationApi -> database "reads/writes" "SQL"
            dataCaptureApi -> database "reads/writes" "SQL"
            dataSharingApi -> database "reads/writes" "SQL"
        }
        oneLogin = softwareSystem "Gov.uk One Login" "Let users sign in and prove their identities to use your service"
        fts = softwareSystem "Find a Tender"
        cfs = softwareSystem "Supplier Information and Contracts Finder"
        ppg = softwareSystem "Public Procurement Gateway"

        supplier -> webApp "Uses" "HTTPS"
        buyer -> eSender "Uses"

        cdp -> oneLogin "Authenticates with"

        eSender -> dataSharingApi "Looks up supplier information" "HTTPS/json"
        fts -> cdp "Authorizes with"
        cfs -> cdp "Authorizes with"
        ppg -> cdp "Authorizes with"
    }

    views {
        systemContext cdp "CDP-SystemContext" {
            include *
        }
        container cdp "CDP-ContainerView" {
            include *
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