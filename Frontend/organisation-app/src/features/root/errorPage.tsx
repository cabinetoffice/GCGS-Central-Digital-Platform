import { isRouteErrorResponse, useRouteError } from "react-router-dom";
import Header from "./header";
import Footer from "./footer";

const ErrorPage = () => {
    const error = useRouteError();

    const ErrorContent = () => {
        if (isRouteErrorResponse(error)) {
            return (
                <>
                    {error.status === 404 ?
                        <>
                            <h1 className="govuk-heading-xl">Page not found</h1>
                            <p className="govuk-body">If you typed the web address, check it is correct.</p>
                            <p className="govuk-body">If you pasted the web address, check you copied the entire address.</p>
                        </>
                        :
                        <>
                            <h1 className="govuk-heading-xl">OOPS! {error.status}</h1>
                            <p className="govuk-body">{error.statusText}</p>
                            {error.data?.message && (
                                <p className="govuk-body">
                                    <i>{error.data.message}</i>
                                </p>
                            )}
                        </>
                    }
                </>
            );
        } else if (error instanceof Error) {
            return (
                <>
                    <h1 className="govuk-heading-xl">OOPS! UNEXPECTED ERROR</h1>
                    <p className="govuk-body">Something went wrong.</p>
                    <p className="govuk-body">
                        <i>{error.message}</i>
                    </p>
                </>
            );
        } else {
            return <></>;
        }
    };

    return (
        <>
            <Header />
            <div className="govuk-width-container">
                <main className="govuk-main-wrapper" id="main-content" role="main">
                    {ErrorContent()}
                </main>
            </div>
            <Footer />
        </>
    );
}

export default ErrorPage;