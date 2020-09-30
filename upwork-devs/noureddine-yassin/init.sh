#!/bin/bash
declare -a IPS=${IPS[@]}
if [ ! -d kubespray ] ; then
    DIR=$(curl -s https://api.github.com/repos/kubernetes-sigs/kubespray/releases/latest | grep "tarball_url" | cut -d : -f 2,3 | \
    tr -d \" | tr -d "," | wget -qi - -O- | tar xzv | tac | tail -n 1 ) # Download latest Kubespray release, extract it and move to "kubespray"
    mv $DIR kubespray
    cp -f kubespray/ansible.cfg . 
fi 
pip3 install -r kubespray/requirements.txt
CONFIG_FILE=cluster/hosts.yaml python3 kubespray/contrib/inventory_builder/inventory.py $IP
