import { NavLink } from "react-router-dom"

const OrganisationTypePage = () =>
    <>
        <NavLink to="/supplier-registration/your-details" className="govuk-back-link">
            Back
        </NavLink>

        <main className="govuk-main-wrapper">
            <div className="govuk-grid-row">
                <div className="govuk-grid-column-two-thirds">
                    <h1 className="govuk-heading-xl">
                        Supplier Registration
                    </h1>

                    <div className="govuk-form-group">
                        <fieldset className="govuk-fieldset">
                            <legend className="govuk-fieldset__legend govuk-fieldset__legend--l">
                                <h1 className="govuk-fieldset__heading">
                                    Are you a supplier, buyer, or both?
                                </h1>
                            </legend>
                            <div className="govuk-radios" data-module="govuk-radios">
                                <div className="govuk-radios__item">
                                    <input className="govuk-radios__input" id="supplier" name="supplier-type" type="radio" value="supplier" />
                                    <label className="govuk-label govuk-radios__label" htmlFor="supplier">
                                        Supplier
                                    </label>
                                </div>
                                <div className="govuk-radios__item">
                                    <input className="govuk-radios__input" id="buyer" name="supplier-type" type="radio" value="buyer" />
                                    <label className="govuk-label govuk-radios__label" htmlFor="buyer">
                                        Buyer
                                    </label>
                                </div>
                                <div className="govuk-radios__item">
                                    <input className="govuk-radios__input" id="both" name="supplier-type" type="radio" value="both" />
                                    <label className="govuk-label govuk-radios__label" htmlFor="both">
                                        Both
                                    </label>
                                </div>
                            </div>
                        </fieldset>
                    </div>

                    <button type="submit" className="govuk-button" data-module="govuk-button">
                        Save and continue
                    </button>
                </div>
            </div>
        </main>
    </>

export default OrganisationTypePage