for DOMAIN in \
www-staging.find-tender.service.gov.uk \
www-tpp.find-tender.service.gov.uk \
www.find-tender.service.gov.uk \
www-preview.contractsfinder.service.gov.uk \
www-integration.contractsfinder.service.gov.uk \
www.contractsfinder.service.gov.uk
do
echo "==> ${DOMAIN}"
echo | openssl s_client \
  -connect ${DOMAIN}:443 \
  -servername ${DOMAIN} 2>/dev/null \
  | openssl x509 -noout -dates
echo ""
done
