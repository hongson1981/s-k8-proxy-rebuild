# Deploy Reverse Proxy Setup on Kubernetes

## Install kubernetes

### kubespray

Kubespray is a composition of Ansible playbooks that helps to install a Kubernetes cluster hosted on GCE, Azure, OpenStack, AWS, vSphere, Packet (bare metal), Oracle Cloud Infrastructure (Experimental) or Baremetal.

### Requirements

1- Ansible v2.9 and python-netaddr is installed on the machine that will run Ansible commands.

2- Jinja 2.11 (or newer) is required to run the Ansible Playbooks.

3- The target servers must have access to the Internet in order to pull docker images.

4- The target servers are configured to allow IPv4 forwarding.

5- Your ssh key must be copied to all the servers part of your inventory.

6- The firewalls are not managed, you'll need to implement your own rules the way you used to, in order to avoid any issue during deployment you should disable your firewall.

7- If kubespray is ran from non-root user account, correct privilege escalation method should be configured in the target servers. Then the ansible_become flag or command parameters --become or -b should be specified.

### Installation

1- The VM running K8s will be hosted on GCP with the following specs:

- Ubuntu 20.04
- e2-medium (2 vCPUs, 4 GB memory)

3- Install needed packages:

```
sudo apt update
sudo apt install ansible python3-pip
git clone https://github.com/kubernetes-sigs/kubespray.git
cd kubespray/
sudo pip3 install -r requirements.txt
```

4- Build the inventory file ([ansible](https://github.com/kubernetes-sigs/kubespray/blob/master/docs/ansible.md)):

  - Generate the inventory file
 
    ```
    declare -a IPS=($k8s_node)
    CONFIG_FILE=inventory/mycluster/hosts.yaml python3 contrib/inventory_builder/inventory.py ${IPS[@]}
    ```

  - Remove access_ip from the _inventory/mycluster/hosts.yaml_ file:

	```
	all:
	  hosts:
	    node1:
	      ansible_host: $k8s_node
	      ip: $k8s_node
	  children:
	    kube-master:
	      hosts:
	        node1:
	    kube-node:
	      hosts:
	        node1:
	    etcd:
	      hosts:
	        node1:
	    k8s-cluster:
	      children:
	        kube-master:
	        kube-node:
	    calico-rr:
	      hosts: {}
	```

5- Add Ingress addon by editing the following lines to _inventory/mycluster/group_vars/k8s-cluster/addons.yml_:

```
ingress_nginx_enabled: true
ingress_nginx_nodeselector:
  node-role.kubernetes.io/master: ""
```

6- Generate SSH key and copy the public key to _~/.ssh/authorized_keys_:

```
ssh-keygen
cat ~/.ssh/id_rsa.pub >> ~/.ssh/authorized_keys 
```

7- Run the ansible playbook:

  `ansible-playbook -i inventory/mycluster/hosts.yaml  --become --become-user=root cluster.yml`

8- Test the cluster deployment:

- SSH to the node and run the following command:
  ```
  sudo su - 
  kubectl get nodes
  kubectl get all --all-namespaces
  ``` 

## Apps deployment


### Clone the https://github.com/k8-proxy/k8-reverse-proxy repository and checkout `pranay/k8s-setup` branch

```
git clone https://github.com/k8-proxy/k8-reverse-proxy.git
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
