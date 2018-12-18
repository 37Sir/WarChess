#!/bin/sh
cd /usr/app/gateway/log
touch mt.out
chown -R kpgame:kpgame mt.out

cd /usr/app/gateway/
chmod +x startup
./startup
