import React from 'react';
import ReactDOM from 'react-dom/client';
import { createBrowserRouter, Navigate, RouterProvider } from "react-router-dom";
import reportWebVitals from './reportWebVitals';

import 'govuk-frontend/dist/govuk/all.scss'

import ErrorPage from 'features/root/errorPage';
import RootLayout from 'features/root/rootLayout';
import HomePage from 'features/root/homePage';
import SupplierRegistrationDetailsPage from 'features/supplier-registration/yourDetailsPage';
import OrganisationTypePage from 'features/supplier-registration/organisationTypePage';

const router = createBrowserRouter([
    {
        path: "/",
        element: <RootLayout />,
        errorElement: <ErrorPage />,
        children: [
            { index: true, element: <HomePage /> },
            {
                path: "supplier-registration",
                children: [
                    { path: "", element: <Navigate to="/supplier-registration/your-details" replace /> },
                    { path: "your-details", element: <SupplierRegistrationDetailsPage /> },
                    { path: "organisation-type", element: <OrganisationTypePage /> }
                ]
            }
        ]
    }
]);

const root = ReactDOM.createRoot(
    document.getElementById('root') as HTMLElement
);

root.render(
    <React.StrictMode>
        <RouterProvider router={router} />
    </React.StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
