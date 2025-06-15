FROM amazon/aws-for-fluent-bit:latest

COPY configs/ /fluent-bit/etc/

RUN chmod +x /entrypoint.sh

#ENTRYPOINT ["/fluent-bit/etc/custom-entrypoint.sh"]
