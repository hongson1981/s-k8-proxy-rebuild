# Deploy Reverse Proxy Setup on Kubernetes

### Clone the https://github.com/k8-proxy/k8-reverse-proxy repository and checkout `pranay/k8s-setup` branch

```
git clone https://github.com/k8-proxy/k8-reverse-proxy.git
git checkout pranay/k8s-setup branch
cd stable-src
```

### Build and push docker images to a container registry. Below example commands show pushing docker images to dockerhub.

```
docker build nginx -t pranaysahith/reverse-proxy-nginx:0.0.1
docker push pranaysahith/reverse-proxy-nginx:0.0.1

docker build squid -t pranaysahith/reverse-proxy-squid:0.0.1
docker push pranaysahith/reverse-proxy-squid:0.0.1

docker build c-icap -t pranaysahith/reverse-proxy-c-icap:0.0.1
docker push pranaysahith/reverse-proxy-squid:0.0.1
```

### Deploy to Kubernetes
From this directory run below commands to deploy the helm chart. If you don't want to build the docker images, it uses the exisiting images given in chart/values.yaml

```
helm upgrade --install reverse-proxy chart/
```

Verify that all pods(nginx, squid, icap) are running by executing below command
```
kubectl get pods
```

Once all the pods are running, forward the traffic from local machine to nginx service.
```
kubectl port-forward svc/reverse-proxy-reverse-proxy-nginx 443:443
```

You have to assign all proxied domains to the localhost address by adding them to hosts file ( `C:\Windows\System32\drivers\etc\hosts` on Windows , `/etc/hosts` on Linux )
  for example: 

```
127.0.0.1 gov.uk.glasswall-icap.com www.gov.uk.glasswall-icap.com assets.publishing.service.gov.uk.glasswall-icap.com
```

You can test the stack functionality by downloading [a rich PDF file](https://assets.publishing.service.gov.uk.glasswall-icap.com/government/uploads/system/uploads/attachment_data/file/901225/uk-internal-market-white-paper.pdf) through the proxy and testing it against [file-drop.co.uk](https://file-drop.co.uk)
You can also visit [the proxied main page](https://www.gov.uk.glasswall-icap.com/) and make sure that the page loads correctly and no requests/links is pointing to the original `*.gov.uk` or other malformed domains.

### Customize deployment configuration
chart/values.yaml file can be updated to pass different environment variables, docker image repository and image tag to use.

for e.g. ALLOWED_DOMAINS, ROOT_DOMAIN, SUBFILTER_ENV etc. environment variable values can be updated to use a different domain.
