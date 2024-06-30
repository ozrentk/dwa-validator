TODO:

- za studente
    - poslati koji je korisnik admin
    - poslati koji je korisnik "obièan korisnik"
    - ako postoje bin i obj folderi, tražiti da ih se obriše zbog problema s velièinom arhive

- dodatne funkcionalnosti
    - kod prvog pokretanja se otvara dijalog i sprema mjesto gdje æe biti raspakirana rješenja i kreirane datoteke baza podataka
      (default: %USERPROFILE%\DwaValidator)
        - treba mi konfiguracija za to
    - podržati scheme kod rada s bazom podataka

- provjere kvalitete: Web API (app i DB)
	- sve provjere kvalitete mogu biti poluautomatizirane, to jest može se prikazati hint
    - postoji li ruta za primarni entitet u bazi?
    - radi li GET na primarnom entitetu?
    - rade li PUT/POST/DELETE na primarnom entitetu?
    - radi li search po parametru Name sa pagingacijom?
    - radi li kreiranje logova i dohvat posljednjih N logova, s time da je N default=10?
    - radi li count logova?
    - imaju li logovi Id, Timestamp, Level i Message?

- provjere kvalitete: MVC
    - radi li landing page?
    - radi li admin login?
    - na stranici s listom entiteta, ima li tekst za pretraživanje, padajuæi izbornik za 1-to-N stavke, gumb search i previous i next za prethodnih 10 i sljedeæih 10?
    - rade li Details, Add, Edit i Delete?
    - List, Add, Edit i Delete za 1-na-N
    - List, Add, Edit i Delete za M-na-N
    - Jesu li navigacija i logout na svim stranicama osim login/register?
    - Jesu li stranice vizualno dotjerane?
    - Imaju li modeli validaciju?
      (moguæ hint)
    - Mogu li se unijeti prazni unosi negdje, a da to nema smisla?
    - Mogu li se unijeti dupli nazivi?
    - Koriste li labele atribute za oznaèavanje?
      (moguæ hint)
    - Postoji li profil stranica sa Ajax funkcionalnosti?
