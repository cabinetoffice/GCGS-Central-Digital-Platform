// scripts/requests/index.js
import { getOrganisations } from './getOrganisations.js';
import { postOrganisations } from './postOrganisations.js';

export const endpointRegistry = {
  // The key is what you'll specify in ENDPOINTS="..."
  getOrgs: getOrganisations,
  postOrgs: postOrganisations,
  // Add more if you have them, e.g. deleteOrgs, patchOrgs, etc.
};
