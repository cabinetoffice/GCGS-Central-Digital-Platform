FROM clamav/clamav:stable

COPY clamd.conf /etc/clamav/clamd.conf

# Run as the image's unprivileged clamav user rather than root. On start-up the entrypoint creates
# /run/clamav and links /var/lock into /run/lock, which a non-root user can only do if it owns
# them, so prepare both here. clamd and freshclam already run as clamav when started by root, so
# nothing else changes.
RUN install -d -o clamav -g clamav -m 775 /run/clamav \
 && chown clamav:clamav /run/lock

USER clamav
