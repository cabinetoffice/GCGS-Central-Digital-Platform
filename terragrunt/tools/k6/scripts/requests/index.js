import { getOrganisations } from './Organisation/getOrganisations.js';
import { postOrganisations } from './Organisation/postOrganisations.js';
import { getOrganisationByGuid } from './Organisation/getOrganisationByGuid.js';
import { patchOrganisation } from './Organisation/patchOrganisation.js';
import { getOrganisationReviews } from './Organisation/getOrganisationReviews.js';
import { getOrganisationJoinRequests } from './Organisation/getOrganisationJoinRequests.js';
import { postFeedback } from './OrganisationFeedback/postFeedback.js';
import { postContactUs } from './OrganisationFeedback/postContactUs.js';
import { getMouLatest } from './OrganisationMOUs/getMouLatest.js';
import { patchBuyerInformation } from './OrganisationBuyerInformation/patchBuyerInformation.js';
import { getBuyerInformation } from './OrganisationBuyerInformation/getBuyerInformation.js';
import { getConnectedEntities } from './OrganisationConnectedEntity/getConnectedEntities.js';
import { postConnectedEntities } from './OrganisationConnectedEntity/postConnectedEntities.js';
import { getSingleConnectedEntity } from './OrganisationConnectedEntity/getSingleConnectedEntity.js';
import { putConnectedEntity } from './OrganisationConnectedEntity/putConnectedEntity.js';
import { getOrganisationMou } from './OrganisationMOUs/getOrganisationMou.js';
import { getOrganisationApiKeys } from './OrganisationAPIKeys/getOrganisationApiKeys.js';
import { postOrganisationApiKeys } from './OrganisationAPIKeys/postOrganisationApiKeys.js';
import { getDataShare } from './DataShare/getDataShare.js';
import { getDataShareFile } from './DataShare/getDataShareFile.js';
import { getOrganisationShareCodes } from './DataShare/getOrganisationShareCodes.js';
import { getOrganisationWithShareCode } from './DataShare/getOrganisationWithShareCode.js';
import { getTenantLookup } from './Tenant/getTenantLookup.js';
import { getOrganisationParties } from './OrganisationParties/getOrganisationParties.js';
import { getOrganisationPersons } from './OrganisationPersons/getOrganisationPersons.js';
import { patchPerson } from './OrganisationPersons/patchPerson.js';
import { getOrganisationInvites } from './OrganisationPersons/getOrganisationInvites.js';
import { postOrganisationInvite } from './OrganisationPersons/postOrganisationInvite.js';
import { patchOrganisationInvite } from './OrganisationPersons/patchOrganisationInvite.js';
import { getOrganisationSupplierInformation } from './OrganisationSupplierInfo/getOrganisationSupplierInformation.js';
import { patchOrganisationSupplierInformation } from './OrganisationSupplierInfo/patchOrganisationSupplierInformation.js';
import { patchOrganisationReview } from './OrganisationSupport/patchOrganisationReview.js';
import { postPersons } from './Persons/postPersons.js';
import { getPersonById } from './Persons/getPersonById.js';
import { postTenant } from './Tenant/postTenant.js';
import { getOrganisationLookupByName } from './OrganisationLookup/getOrganisationLookupByName.js';
import { putAnswersAndShareData } from './DataShare/putAnswersAndShareData.js';
import { verifySharedData } from './DataShare/verifySharedData.js';

export const endpointRegistry = {
  
  getOrganisations,
  postOrganisations,
  getOrganisationByGuid,
  patchOrganisation,
  getOrganisationReviews,
  getOrganisationJoinRequests,
  postFeedback,
  postContactUs,
  getMouLatest,
  patchBuyerInformation,
  getBuyerInformation,
  getConnectedEntities,
  postConnectedEntities,
  getSingleConnectedEntity,
  putConnectedEntity,
  getOrganisationMou,
  getOrganisationApiKeys,
  postOrganisationApiKeys,
  getDataShare,
  getDataShareFile,
  getOrganisationShareCodes,
  getOrganisationWithShareCode,
  getTenantLookup,
  postTenant,
  getOrganisationParties,
  getOrganisationPersons,
  patchPerson,
  getOrganisationInvites,
  postOrganisationInvite,
  patchOrganisationInvite,
  getOrganisationSupplierInformation,
  patchOrganisationSupplierInformation,
  patchOrganisationReview,
  postPersons,
  getPersonById,
  getOrganisationLookupByName,
  putAnswersAndShareData,
  verifySharedData

};