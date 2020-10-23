# reverse-proxy-icap-docker (Release 2) -Kubernetes deployment

[Source](https://github.com/k8-proxy/k8-reverse-proxy)

Release 2 includes the completed/tested pieces of the Reverse Proxy project as a "reverse-proxy-icap-docker" to run inside a standard Ubuntu 18.04 server OVA image. The completed pieces of this project so far are:

- Squid based reverse Proxy for a specific website, with ICAP integration.
- Two Way URL rewrite/ with the help of NGINX in front of Squid.
- SSL termination.
- GW Rebuild ICAP service

**Default configuration includes proxy for**

- assets.publishing.service.gov.uk.glasswall-icap.com
- gov.uk.glasswall-icap.com
- www.gov.uk.glasswall-icap.com
- Built-in GW Rebuild ICAP service, can be change by setting **ICAP_URL** in `gwproxy.env`

## Preparing environment:
### Install Docker&docker-compose
```bash
sudo apt-get update && sudo apt-get install curl git -y
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo curl -L "https://github.com/docker/compose/releases/download/1.27.0/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```
### Install Minikube
```
curl -Lo minikube https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64 \
&& chmod +x minikube
sudo mkdir -p /usr/local/bin/
sudo install minikube /usr/local/bin/
start minikube by command :
minikube start
```
### Install Kompose
Linux
```
  curl -L https://github.com/kubernetes/kompose/releases/download/v1.21.0/kompose-linux-amd64 -o kompose
  chmod +x kompose
  sudo mv ./kompose /usr/local/bin/kompose
```
## Preparing source code

0. Clone repository and goto the stable-src

```bash
git clone --recursive https://github.com/k8-proxy/k8-reverse-proxy
cd stable-src
git submodule update # Update submodules
wget -O c-icap/Glasswall-Rebuild-SDK-Evaluation/Linux/Library/libglasswall.classic.so https://raw.githubusercontent.com/filetrust/Glasswall-Rebuild-SDK-Evaluation/master/Linux/Library/libglasswall.classic.so # Get latest evaluation build of GW Rebuild engine
```

1. If you are deploying the proxy for other websites, tweak `subfilter.sh` to rewrite URLs in backend response in the following format
   You can modify the file using `nano subfilter.sh`, save and exit using`CTRL+X`, then `Y`

```bash
SUBFILTER=( PATTERN_TO_MATCH1,PATTERN_REPLACE1 PATTERN_TO_MATCH2,PATTERN_REPLACE2 )
```

The default configuration is:

```bash
SUBFILTER=( .gov.uk,.gov.uk.glasswall-icap.com  .amazonaws.com,.amazonaws.com.glasswall-icap.com )
```

This means that any occurence of **.gov.uk** in the response should be replaced with **.gov.uk.glasswall-icap.com** , and **.amazonaws.com** will be replaced with **.amazonaws.com.glasswall-icap.com** .

2. If you are deploying the proxy for other websites, tweak `gwproxy.env`:
   **ALLOWED_DOMAINS** variable to include a comma-separated list of proxied domains, any requests to other domains will be denied.
   
   Set **ROOT_DOMAIN** to match the domain used as suffix for backend domains.
   
   You can modify the file using `nano gwproxy.env`, save and exit using`CTRL+X`, then `Y`

```bash
# Allowed requested domains, comma-separated
ALLOWED_DOMAINS=gov.uk.glasswall-icap.com,www.gov.uk.glasswall-icap.com,assets.publishing.service.gov.uk.glasswall-icap.com
# ICAP server url
ICAP_URL=icap://127.0.0.1:1344/gw_rebuild
# Root domain, the domain appended to the backend website websitedomain
ROOT_DOMAIN=glasswall-icap.com
```

3. Change environment to upload docker file to docker hub
  Edit docker-compose.yml
  find and replace "image:<docker image> " by "image:<your docker hub username>/<docker image> "
   example :"image: squid-reverse" -> "image: sptit145/squid-reverse"  (sptit145 is my dockerhub username)
   ```
## Deployment

4. Login to docker hub
   
   ```bash
   docker login
   ```
5. Check kubernetes environment 
   ```
    kubectl cluster-info 
    Get kubernetes IP:PORT
    ```
6. Deploy kubernetes POD
   ```
   cd <folder of docker-compose.yml file>
   kompose up --server <kubernetes IP:PORT>
   ```
## Troubleshooting

- Check if kubernetes service is active
  
  ```bash
  kubectl get services
  ```

  ```

- If squid or nginx is not started correctly, or any of the configuration parameters in `gwproxy.env` or `subfilter.sh` has been modified, execute:
  
  ```bash
  sudo docker-compose up -d --force-recreate
  ```

- You have to assign all proxied domains to the docker host ip address by adding them to hosts file ( `C:\Windows\System32\drivers\etc\hosts` on Windows , `/etc/hosts` on Linux )
  for example: 

```
192.168.0.10 gov.uk.glasswall-icap.com www.gov.uk.glasswall-icap.com assets.publishing.service.gov.uk.glasswall-icap.com
```

where `192.168.0.10` is an example for the IP address for the docker host (i.e Cloud or local VM), You will need to replace domains with domains in **ALLOWED_DOMAINS** from `gwproxy.env` in case you are deploying to target another backend site.

Make sure you are able to connect to the docker host and that no firewall/routes rules are preventing connections to TCP ports 80 or 443.

You can test the stack functionality by downloading [a rich PDF file](https://assets.publishing.service.gov.uk.glasswall-icap.com/government/uploads/system/uploads/attachment_data/file/901225/uk-internal-market-white-paper.pdf) through the proxy and testing it against [file-drop.co.uk](https://file-drop.co.uk)
You can also visit [the proxied main page](https://www.gov.uk.glasswall-icap.com/) and make sure that the page loads correctly and no requests/links is pointing to the original `*.gov.uk` or other malformed domains.
