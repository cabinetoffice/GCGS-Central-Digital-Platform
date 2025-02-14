// scripts/endpoints/putAnswersAndShareData.js
import http from 'k6/http';
import { check } from 'k6';
import {
  ORG_GUID,
  FORM_ID,
  SECTION_ID,
  ANSWER_GUID,
} from '../config.js';

// 1) PUT answers
export function putAnswers({ token, domain }) {
  const url = `https://forms.${domain}/forms/${FORM_ID}/sections/${SECTION_ID}/answers/${ANSWER_GUID}?organisation-id=${ORG_GUID}`;

  const params = {
    headers: {
      'Content-Type': 'application/json',
      accept: '*/*',
    },
  };
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  const body = JSON.stringify({
    answers: [
      {
        id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        questionId: 'f4a5b6c7-8d9e-0f1a-2b3c-456789def012',
        boolValue: true,
      },
    ],
    furtherQuestionsExempted: true,
  });

  const resPut = http.put(url, body, params);

  check(resPut, {
    'PUT answers is 200 or 201': (r) => r.status === 204 || r.status === 201,
  });

  return resPut;
}

// 2) POST share data
export function postShareData({ token, domain }) {
  const url = `https://data-sharing.${domain}/share/data`;

  const params = {
    headers: {
      'Content-Type': 'application/json',
      accept: 'application/json',
    },
  };
  if (token) {
    params.headers.Authorization = `Bearer ${token}`;
  }

  const body = JSON.stringify({
    formId: FORM_ID,
    organisationId: ORG_GUID,
  });

  const resPost = http.post(url, body, params);

  check(resPost, {
    'POST share data is 200 or 201': (r) => r.status === 200 || r.status === 201,
  });

  return resPost;
}

// 3) Helper function that triggers both in sequence
export function putAnswersAndShareData({ token, domain }) {
  // First do the PUT
  const resPut = putAnswers({ token, domain });
  // Then do the POST
  const resPost = postShareData({ token, domain });
  
  // Return both if you want to see statuses outside
  return { resPut, resPost };
}
