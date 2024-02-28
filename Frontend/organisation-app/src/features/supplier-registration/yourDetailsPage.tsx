import { NavLink } from "react-router-dom"

const SupplierRegistrationDetailsPage = () =>
    <>
        <NavLink to="/" className="govuk-back-link">
            Back
        </NavLink>

        <main className="govuk-main-wrapper">
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <h1 className="govuk-heading-xl">
                        Supplier Registration
                    </h1>

                    <fieldset className="govuk-fieldset">
                        <legend className="govuk-fieldset__legend govuk-fieldset__legend--l">
                            <h1 className="govuk-fieldset__heading">
                                Enter your details?
                            </h1>
                        </legend>

                        <div className="govuk-form-group">
                            <label className="govuk-label" htmlFor="first-name">
                                First name
                            </label>
                            <input className="govuk-input" id="first-name" name="firstName" type="text" />
                        </div>

                        <div className="govuk-form-group">
                            <label className="govuk-label" htmlFor="last-name">
                                Last name
                            </label>
                            <input className="govuk-input" id="last-name" name="lastName" type="text" />
                        </div>
                    </fieldset>

                    <NavLink to="/supplier-registration/organisation-type">
                        <button type="submit" className="govuk-button" data-module="govuk-button">
                            Save and continue
                        </button>
                    </NavLink>
                </div>
            </div>
        </main>
    </>

export default SupplierRegistrationDetailsPage