FROM squid-reverse:latest
RUN apt update
RUN apt install dnsutils -y
COPY ./entrypoint.sh /entrypoint.sh
RUN chmod 755 /entrypoint.sh
