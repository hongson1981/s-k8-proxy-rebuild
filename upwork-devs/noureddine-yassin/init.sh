#!/bin/bash
export CONFIG_FILE=cluster/hosts.yaml
git clone https://github.com/kubernetes-sigs/kubespray
pip3 install -r kubespray/requirements.txt
python3 kubespray/contrib/inventory_builder/inventory.py $IP
