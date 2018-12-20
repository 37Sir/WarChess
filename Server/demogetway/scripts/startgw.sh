#!/bin/sh
cd /usr/app/demogetway/log
touch mt.out
chown -R kpgame:kpgame mt.out

cd /usr/app/demogetway/
chmod +x startup
./startup
