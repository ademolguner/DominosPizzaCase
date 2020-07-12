FROM mcr.microsoft.com/mssql/server:2017-latest
COPY . /

RUN chmod +x ./db1/db-init.sh
CMD /bin/bash ./db1/entrypoint.sh