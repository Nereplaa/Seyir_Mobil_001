# Gelistirme icin: "ng serve" konteyner icinde calisiyor, host'taki "npm start"in
# konteynerlestirilmis karsiligi. Kaynak kod docker-compose'da bir volume olarak bagliyor,
# image sadece npm bagimliliklarini onceden yukluyor (build hizi icin).
FROM node:24

WORKDIR /src

COPY package.json package-lock.json ./
RUN npm install

COPY . .

EXPOSE 4200

# --host 0.0.0.0: konteyner disindan (host makine, tarayici) erisilebilmesi icin - varsayilan
# "ng serve" sadece localhost'u dinler, bu da konteyner icinden disariya kapali olur.
# --poll: bazi Docker/host dosya sistemi kombinasyonlarinda (ozellikle Windows bind mount)
# native dosya izleme (inotify) calismiyor, poll modu HMR'nin guvenilir calismasini saglar.
CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--poll", "2000"]
