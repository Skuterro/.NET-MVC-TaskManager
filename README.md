# .NET-MVC-TaskManager

# Spis treści
- [O projekcie](#o-projekcie)
- [Uruchomienie](#uruchomienie)

## O projekcie
Aplikacja do zarządzania zadaniami zbudowana w oparciu o wzorzec MVC z wykorzystaniem ASP.NET Core, Entity Framework Core, PostgreSQL oraz interaktywnego interfejsu użytkownika z JavaScript i elementami drag & drop. Aplikacja jest skonteneryzowana za pomocą Docker i Docker Compose.

Pozwala użytkownikowi z prawami administratora na:
- tworzenie, edycję oraz usuwanie wszystkich zadań w systemie
- wyświetlanie wszystkich zadań
- przydzielać zadania innym użytkownikom
- zmianę statusu każdego zadania w systemie
- podgląd historii (logów) zmian w zadaniach tzn. kto, kiedy i co edytował. Z możliwością filtrowania po nazwie zadania lub przypisanym użytkowniku.

Natomiast użytkownik bez praw administratora może tylko edytować status zadań, które są do niego przypisane oraz je wyświetlać.

![image](https://github.com/user-attachments/assets/1254bd10-8c84-4038-a2ef-3eade78c9ea0)

Zmiana statusu zadania polega na przeciągnięciu go do odpowiedniej kolumny.

![image](https://github.com/user-attachments/assets/8b802110-fe66-41f3-a140-75960f2903bd)

![image](https://github.com/user-attachments/assets/17d0d70a-12fb-4dfc-a8f8-07ac32f187bd)

Wszystkie akcje związane z zadaniami są zapisywane w historii.

![image](https://github.com/user-attachments/assets/6aa5e4c9-486c-4ec9-827e-18830844201b)


## Uruchomianie
Aplikacja jest skonfigurowana w taki sposób, że po uruchomieniu automatycznie stosowane są dostępne migracje bazy danych oraz tworzony jest użytkownik - admin.
Aby uruchomić aplikację należy w głównym folderze projektu (tam gdzie jest plik .sln) utworzyć plik ```.env``` na wzór ```.env.example.```
Następnie należy wpisać komendę:
```sh
docker-compose up --build
```
Oraz w przeglądarce wpisać adres:
```sh
http://localhost:8080/
```
