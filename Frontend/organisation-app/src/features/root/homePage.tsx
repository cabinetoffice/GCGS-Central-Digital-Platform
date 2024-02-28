import { NavLink } from "react-router-dom"

const HomePage = () =>
    <>
        <main className="govuk-main-wrapper">
            <h1 className="govuk-heading-xl">Central Digital Platform</h1>

            <h2 className="govuk-heading-l">Supplier Registration</h2>

            <NavLink to="/supplier-registration/your-details" >
                <button type="submit" className="govuk-button" data-module="govuk-button">
                    Register
                </button>
            </NavLink>
        </main>
    </>

export default HomePage