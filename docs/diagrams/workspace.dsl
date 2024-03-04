workspace "Central Digital Platform" {

    model {
        supplier = person "Supplier"
        buyer = person "Buyer"
        eSender = softwareSystem "eSender" "Commercial Software that Buyers use to manage tender processes"
        cdp = softwareSystem "Central Digital Platform" "Support procurement"
        oneLogin = softwareSystem "Gov.uk One Login" "Let users sign in and prove their identities to use your service"
        fts = softwareSystem "Find a Tender"
        cfs = softwareSystem "Supplier Information and Contracts Finder"
        ppg = softwareSystem "Public Procurement Gateway"

        supplier -> cdp "Uses"
        buyer -> eSender "Uses"

        cdp -> oneLogin "Authenticates with"

        eSender -> cdp "Looks up supplier information"
        fts -> cdp "Authorizes with"
        cfs -> cdp "Authorizes with"
        ppg -> cdp "Authorizes with"
    }

    views {
        systemContext cdp "CDP-SystemContext" {
            include *
        }
        theme default
    }

    configuration {
        scope softwaresystem
    }

}