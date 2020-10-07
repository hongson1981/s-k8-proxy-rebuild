#!/bin/bash
if [ -f /first-run ] ; then
	IP=$(dig +short "$ICAP_FQDN")
	>/etc/squid/conf.d/allowed_backends.conf
	for domain in $( echo $ALLOWED_DOMAINS | tr ',' ' ' ) ; do
	    echo "acl allowed dstdomain $domain" >> /etc/squid/conf.d/allowed_backends.conf
	done
        ICAP_URL=icap://"$IP":"$ICAP_PORT"/gw_rebuild 
	sed -i "s,ICAP_URL,$ICAP_URL,g" /etc/squid/squid.conf
	sed -i "s,ROOT_DOMAIN,$ROOT_DOMAIN,g" /etc/squid/rewriter
	rm /first-run
fi
/usr/sbin/squid --foreground -f /etc/squid/squid.conf
