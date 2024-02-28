import { Outlet } from "react-router-dom";
import Header from "./header";
import Footer from "./footer";

const RootLayout = () =>
    <>
        <Header />
        <div className="govuk-width-container">
            <Outlet />
        </div>
        <Footer />
    </>

export default RootLayout;