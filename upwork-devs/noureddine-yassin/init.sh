#!/bin/bash
CONFIG_FILE=cluster/hosts.yaml
git clone https://github.com/kubernetes-sigs/kubespray
python3 kubespray/contrib/inventory_builder/inventory.py ${IPS[@]}

