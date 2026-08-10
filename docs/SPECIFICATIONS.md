# Cahier des charges — Application de gestion financière personnelle

## 1. Présentation du projet

Créer une application moderne de gestion financière personnelle permettant à un utilisateur ou à un foyer de suivre ses revenus, dépenses, comptes, budgets, objectifs d’épargne et prévisions financières.

L'application doit être disponible sous deux formes :

* Application Web responsive
* Application Mobile iOS / Android

Le projet doit être conçu comme une application réelle destinée à évoluer, avec :

* architecture microservices
* architecture événementielle
* DDD
* CQRS lorsque pertinent
* communication asynchrone
* sécurité forte
* observabilité
* tests automatisés
* conteneurisation
* CI/CD

Nom de travail du projet :

**FinanceOS**

---

# 2. Objectifs principaux

L'application doit permettre de :

1. gérer plusieurs comptes financiers ;
2. ajouter et catégoriser des transactions ;
3. suivre les revenus et dépenses ;
4. définir des budgets mensuels ;
5. suivre la consommation des budgets ;
6. gérer les dépenses et revenus récurrents ;
7. définir des objectifs d'épargne ;
8. visualiser des tableaux de bord financiers ;
9. prévoir la situation financière en fin de mois ;
10. recevoir des alertes ;
11. partager les finances au niveau d'un foyer ;
12. disposer d'une application mobile synchronisée.

---

# 3. Utilisateurs et foyers

## 3.1 Utilisateur

Un utilisateur possède :

* Id
* prénom
* nom
* email
* devise préférée
* langue
* timezone
* date de création
* préférences

---

## 3.2 Household

Un utilisateur appartient à au moins un foyer.

Entity :

Household

Champs :

* Id
* Name
* Currency
* CreatedAt
* OwnerId

Un foyer peut contenir plusieurs utilisateurs.

---

## 3.3 Membership

Un membre possède un rôle :

* Owner
* Admin
* Member
* Viewer

Permissions :

Owner :

* accès complet
* gestion du foyer
* gestion des membres

Admin :

* gestion finances
* gestion budgets
* gestion catégories

Member :

* création / modification de transactions

Viewer :

* lecture uniquement

Toutes les données financières doivent être isolées par HouseholdId.

Aucun utilisateur ne doit pouvoir accéder aux données d'un autre foyer.

---

# 4. Gestion des comptes financiers

L'utilisateur peut créer plusieurs comptes.

Types :

* Checking
* Savings
* Cash
* CreditCard
* Loan
* Investment
* Other

Account :

* Id
* HouseholdId
* Name
* Type
* Currency
* InitialBalance
* CurrentBalance
* InstitutionName
* IsActive
* CreatedAt
* UpdatedAt

Fonctionnalités :

* créer un compte
* modifier un compte
* archiver un compte
* consulter le solde
* consulter l'historique des transactions

---

# 5. Gestion des transactions

Une transaction représente un mouvement financier.

Types :

* Expense
* Income
* Transfer
* Refund

Transaction :

* Id
* HouseholdId
* AccountId
* DestinationAccountId
* Type
* Amount
* Currency
* CategoryId
* Merchant
* Description
* TransactionDate
* CreatedAt
* UpdatedAt
* IsRecurring
* RecurringTransactionId
* Tags

Contraintes :

Amount > 0.

Le type définit la manière dont le montant affecte le compte.

Expense :

solde -= montant.

Income :

solde += montant.

Transfer :

source -= montant.

destination += montant.

Refund :

solde += montant.

---

# 6. Catégories

Les transactions peuvent être catégorisées.

Exemples :

Housing

Food

Groceries

Transport

Car

Health

Childcare

Pets

Entertainment

Shopping

Subscriptions

Travel

Education

Salary

Benefits

Investment

Other

Une catégorie peut contenir des sous-catégories.

Category :

* Id
* HouseholdId
* Name
* ParentCategoryId
* Icon
* IsSystem
* CreatedAt

Les catégories système ne peuvent pas être supprimées.

Les utilisateurs peuvent créer leurs propres catégories.

---

# 7. Tags

Une transaction peut contenir plusieurs tags.

Exemples :

vacances

travail

famille

bébé

voiture

Tag :

* Id
* HouseholdId
* Name

---

# 8. Revenus

Les revenus sont des transactions de type Income.

Exemples :

* salaire
* prime
* allocation
* remboursement
* revenu secondaire
* revenu exceptionnel

L'application doit permettre de différencier :

* revenus récurrents
* revenus exceptionnels

---

# 9. Budgets

L'utilisateur doit pouvoir définir un budget mensuel.

Budget :

* Id
* HouseholdId
* Year
* Month
* TotalBudget
* CreatedAt

BudgetAllocation :

* Id
* BudgetId
* CategoryId
* PlannedAmount
* ActualAmount

Exemple :

Courses : 500 €

Transport : 150 €

Restaurant : 150 €

Enfant : 400 €

Animaux : 80 €

Loisirs : 200 €

---

# 10. Calcul du budget

Chaque fois qu'une dépense est créée :

TransactionCreated

Budget Service doit recalculer le montant consommé.

Exemple :

Budget Courses :

500 €.

Dépense créée :

50 €.

Résultat :

50 / 500 € consommés.

Progression :

10 %.

---

# 11. Seuils de budget

Le système doit détecter :

50 %

75 %

90 %

100 %

> 100 %

Lorsque le budget dépasse certains seuils, générer des événements.

BudgetThresholdReached.

BudgetExceeded.

Exemple :

Budget Courses :

500 €.

Dépenses :

455 €.

Émettre :

BudgetThresholdReached

Threshold = 90.

---

# 12. Dépenses récurrentes

RecurringTransaction :

* Id
* HouseholdId
* AccountId
* CategoryId
* Type
* Amount
* Frequency
* NextExecutionDate
* StartDate
* EndDate
* IsActive

Frequency :

* Daily
* Weekly
* Monthly
* Quarterly
* Yearly

Exemples :

loyer

Netflix

assurance

assistante maternelle

crédit

salaire

internet

---

# 13. Prévision financière

Le système doit calculer une projection financière.

Forecast :

Solde actuel

*

revenus prévus

*

dépenses prévues.

Endpoints :

GET /forecast/end-of-month

GET /forecast/30-days

GET /forecast/90-days

Exemple :

Solde actuel :

4 500 €

Revenus prévus :

2 500 €

Dépenses prévues :

3 200 €

Solde estimé :

3 800 €.

---

# 14. Objectifs d'épargne

SavingsGoal :

* Id
* HouseholdId
* Name
* TargetAmount
* CurrentAmount
* TargetDate
* MonthlyTarget
* CreatedAt

Exemples :

fonds d'urgence

vacances

voiture

apport immobilier

investissement.

L'utilisateur doit pouvoir ajouter une contribution.

SavingsContribution :

* Id
* SavingsGoalId
* Amount
* Date

---

# 15. Dashboard

Le dashboard doit afficher au minimum :

Solde global

Revenus du mois

Dépenses du mois

Épargne du mois

Taux d'épargne

Budget consommé

Top catégories

Évolution mensuelle

Dépenses récurrentes

Prévision fin du mois.

---

# 16. Analytics

Prévoir les graphiques suivants :

Dépenses par catégorie.

Revenus vs dépenses.

Évolution mensuelle.

Épargne.

Budget prévu vs réel.

Répartition des comptes.

Dépenses quotidiennes.

---

# 17. Notifications

Types :

BudgetThresholdReached

BudgetExceeded

LowBalance

RecurringPaymentDue

SavingsGoalReached

UnusualExpenseDetected.

Canaux :

In-app.

Push mobile.

Email.

---

# 18. Architecture générale

Architecture cible :

Angular Web

*

Ionic Mobile

↓

API Gateway

↓

Microservices

↓

Event Bus

↓

Read Models / Projections / Notifications.

---

# 19. Architecture microservices

Services initiaux :

Identity Service

Finance Service

Budget Service

Notification Service

Forecast Service.

---

# 20. Identity Service

Responsabilités :

authentification

utilisateurs

foyers

membres

permissions

préférences.

Technologies :

ASP.NET Core.

ASP.NET Identity ou Microsoft Entra External ID.

OAuth2.

OpenID Connect.

JWT.

---

# 21. Finance Service

Responsabilités :

Accounts.

Transactions.

Categories.

Tags.

Recurring Transactions.

---

# 22. Budget Service

Responsabilités :

Budgets.

Budget Allocations.

Budget tracking.

Threshold detection.

---

# 23. Forecast Service

Responsabilités :

Financial forecasts.

Cash flow projections.

Future transactions.

---

# 24. Notification Service

Responsabilités :

In-app notifications.

Push notifications.

Emails.

---

# 25. Découpage futur

Lorsque le projet évolue, Finance Service pourra être découpé en :

Account Service.

Transaction Service.

Recurring Payment Service.

Savings Service.

---

# 26. Architecture interne des microservices

Utiliser :

DDD

*

Clean Architecture

*

Vertical Slice Architecture.

Structure :

src/

ServiceName.Api

ServiceName.Application

ServiceName.Domain

ServiceName.Infrastructure

ServiceName.Contracts.

---

# 27. Domain Layer

Contient uniquement :

Aggregates.

Entities.

Value Objects.

Domain Events.

Domain Exceptions.

Business Rules.

Aucune dépendance Infrastructure.

---

# 28. Application Layer

Contient :

Commands.

Queries.

Handlers.

DTO.

Validators.

Interfaces.

Mappers.

---

# 29. Infrastructure Layer

Contient :

Entity Framework Core.

Repositories.

Message Bus.

Redis.

External APIs.

Persistence.

Telemetry.

---

# 30. API Layer

Utiliser ASP.NET Core Minimal APIs.

Endpoints organisés par feature.

Exemple :

Features/

Transactions/

CreateTransaction.cs

UpdateTransaction.cs

DeleteTransaction.cs

GetTransaction.cs

GetTransactions.cs.

---

# 31. CQRS

Utiliser CQRS pour :

Transactions.

Budgets.

Forecast.

Dashboard.

Séparer :

Commands.

Queries.

Ne pas appliquer CQRS inutilement à chaque fonctionnalité.

---

# 32. Event-Driven Architecture

Message Broker :

RabbitMQ en environnement local.

Azure Service Bus en production.

Framework :

MassTransit.

---

# 33. Événements principaux

TransactionCreated.

TransactionUpdated.

TransactionDeleted.

AccountCreated.

AccountBalanceChanged.

BudgetCreated.

BudgetThresholdReached.

BudgetExceeded.

RecurringTransactionDue.

SavingsContributionAdded.

SavingsGoalReached.

LowBalanceDetected.

---

# 34. Contrats d'événements

Les événements doivent être placés dans des packages Contracts indépendants.

Exemple :

FinanceOS.Contracts.Transactions.

FinanceOS.Contracts.Budgets.

Ne jamais partager les Domain Entities entre microservices.

---

# 35. Exemple TransactionCreated

Contient :

EventId

OccurredAt

TransactionId

HouseholdId

AccountId

Amount

Currency

CategoryId

TransactionDate.

---

# 36. Outbox Pattern

Toute publication d'événement doit utiliser l'Outbox Pattern.

Une transaction DB doit enregistrer simultanément :

la donnée métier

*

OutboxMessage.

Un background worker publie ensuite le message.

L'objectif est d'éviter :

DB success

*

RabbitMQ failure.

---

# 37. Idempotence

Tous les consumers doivent être idempotents.

Créer une table :

InboxMessage.

Elle contient :

MessageId

Consumer

ProcessedAt.

Un même message ne doit jamais modifier deux fois les données.

---

# 38. Eventual Consistency

Accepter l'eventual consistency entre microservices.

Exemple :

une transaction est créée.

Quelques millisecondes plus tard :

le budget est recalculé.

le dashboard est mis à jour.

les notifications sont créées.

---

# 39. Saga

Utiliser une Saga uniquement pour les workflows distribués complexes.

Premier cas :

Transfer Between Accounts.

Étapes :

TransferRequested.

SourceAccountDebited.

DestinationAccountCredited.

TransferCompleted.

Si l'opération échoue :

TransferFailed.

Puis compensation.

---

# 40. API Gateway

Utiliser :

YARP.

Responsabilités :

Routing.

Authentication validation.

Rate limiting.

Correlation ID.

Logging.

---

# 41. Frontend Web

Technologie :

Angular.

Utiliser la version stable actuelle au moment de l'implémentation.

Architecture Angular :

src/app/

core/

shared/

features/

layouts/.

Features :

dashboard

accounts

transactions

budget

savings

forecast

analytics

notifications

settings.

---

# 42. State Management

Utiliser en priorité :

Angular Signals.

RxJS.

Ajouter NgRx Signal Store lorsque la complexité du state le justifie.

Éviter un store global inutile.

---

# 43. UI

UI Library recommandée :

Angular Material.

L'application doit supporter :

Desktop.

Tablet.

Mobile.

Dark Mode.

Light Mode.

---

# 44. Application Mobile

Technologies :

Angular.

Ionic.

Capacitor.

L'application mobile doit partager un maximum de code avec Angular Web.

Fonctionnalités prioritaires :

ajouter une transaction.

consulter dashboard.

consulter budget.

consulter comptes.

recevoir notifications.

ajouter dépense récurrente.

---

# 45. Monorepo

Utiliser Nx.

Structure recommandée :

apps/

web/

mobile/

api-gateway/

services/.

libs/

shared-ui/

auth/

transactions/

budget/

dashboard/

contracts/.

---

# 46. Base de données

Utiliser PostgreSQL.

Chaque microservice possède sa propre base logique.

Interdiction pour un microservice de lire directement la base d'un autre.

Exemple :

identity-db.

finance-db.

budget-db.

forecast-db.

notification-db.

---

# 47. ORM

Entity Framework Core pour :

aggregates.

CRUD métier.

Persistence.

Dapper peut être utilisé pour :

read models.

analytics.

requêtes lourdes.

---

# 48. Redis

Utiliser Redis pour :

cache.

distributed cache.

rate limiting si nécessaire.

eventuellement distributed locks.

---

# 49. Read Models

Créer des projections optimisées pour le dashboard.

Exemple :

MonthlyFinanceSummary.

Champs :

HouseholdId.

Year.

Month.

Income.

Expenses.

Savings.

SavingsRate.

CurrentBalance.

---

# 50. Dashboard Projection

Dashboard doit consommer :

TransactionCreated.

TransactionUpdated.

TransactionDeleted.

AccountBalanceChanged.

Puis mettre à jour ses read models.

---

# 51. Temps réel

Utiliser SignalR pour les mises à jour temps réel.

Exemple :

TransactionCreated

↓

BudgetUpdated

↓

SignalR

↓

Angular Dashboard.

---

# 52. Authentification

Utiliser :

OAuth2.

OIDC.

JWT.

Access Token.

Refresh Token.

---

# 53. Autorisation

Toutes les requêtes doivent vérifier :

UserId.

HouseholdId.

Role.

Un utilisateur ne peut jamais fournir librement un HouseholdId pour accéder à des données arbitraires.

---

# 54. Sécurité

Implémenter :

HTTPS.

CORS.

Rate Limiting.

JWT validation.

Input validation.

Secure headers.

Secrets management.

Audit logs.

Refresh token rotation.

Token expiration.

---

# 55. Validation

Utiliser FluentValidation.

Toutes les commandes doivent être validées.

Exemple :

CreateTransactionCommand.

Amount > 0.

AccountId obligatoire.

TransactionDate obligatoire.

Currency valide.

---

# 56. Gestion des erreurs

Utiliser ProblemDetails RFC 7807.

Exemple :

400 validation error.

401 unauthorized.

403 forbidden.

404 resource not found.

409 business conflict.

500 unexpected error.

---

# 57. Observabilité

Utiliser OpenTelemetry.

Collecter :

Logs.

Metrics.

Traces.

---

# 58. Logging

Utiliser Serilog.

Chaque log doit contenir :

CorrelationId.

TraceId.

UserId si disponible.

HouseholdId si applicable.

ServiceName.

---

# 59. Distributed Tracing

Pouvoir suivre une requête complète :

Angular

↓

Gateway

↓

Finance Service

↓

RabbitMQ

↓

Budget Service

↓

Notification Service.

---

# 60. Monitoring local

Utiliser :

Seq ou Grafana.

OpenTelemetry Collector.

Jaeger ou Tempo.

Prometheus.

---

# 61. Health Checks

Chaque service expose :

/health

/health/live

/health/ready.

---

# 62. Docker

Tous les services doivent disposer d'un Dockerfile.

Créer un docker-compose local contenant :

PostgreSQL.

RabbitMQ.

Redis.

Seq.

API Gateway.

Microservices.

---

# 63. Cloud

Cloud cible :

Microsoft Azure.

Services possibles :

Azure Container Apps.

Azure Service Bus.

Azure Database for PostgreSQL.

Azure Cache for Redis.

Azure Key Vault.

Application Insights.

Azure Blob Storage.

---

# 64. CI/CD

Utiliser GitHub Actions ou Azure DevOps.

Pipeline :

restore.

build.

lint.

unit tests.

integration tests.

docker build.

security scan.

publish image.

deploy.

---

# 65. Tests Backend

Framework :

xUnit.

FluentAssertions.

NSubstitute ou Moq.

Testcontainers.

---

# 66. Tests à prévoir

Unit Tests.

Integration Tests.

Architecture Tests.

Contract Tests.

API Tests.

End-to-End Tests.

---

# 67. Integration Tests

Utiliser Testcontainers pour démarrer :

PostgreSQL.

RabbitMQ.

Redis.

Les tests ne doivent pas dépendre d'une infrastructure manuelle locale.

---

# 68. Tests Frontend

Utiliser :

Angular TestBed.

Vitest si compatible avec la version choisie.

Playwright pour E2E.

---

# 69. Architecture Tests

Créer des tests permettant de garantir :

Domain ne dépend pas de Infrastructure.

Application ne dépend pas de API.

Les services ne partagent pas leurs Entities.

---

# 70. Convention API

Versionner les endpoints.

Exemple :

/api/v1/accounts

/api/v1/transactions

/api/v1/budgets.

---

# 71. Pagination

Toutes les listes potentiellement importantes doivent être paginées.

Query params :

page.

pageSize.

sort.

search.

---

# 72. Dates

Toutes les dates stockées côté backend doivent être UTC.

La conversion locale est réalisée côté frontend.

---

# 73. Argent

Ne jamais utiliser float ou double pour les montants.

Backend :

decimal.

Créer un Value Object Money.

Money contient :

Amount.

Currency.

---

# 74. Audit

Les entités principales doivent contenir :

CreatedAt.

CreatedBy.

UpdatedAt.

UpdatedBy.

---

# 75. Soft Delete

Utiliser soft delete uniquement lorsque cela est pertinent.

Champs :

IsDeleted.

DeletedAt.

---

# 76. Import de données

Prévoir ultérieurement :

CSV import.

Excel import.

Bank transaction import.

L'utilisateur devra pouvoir mapper les colonnes.

---

# 77. Fonctionnalités futures

Prévoir l'architecture pour ajouter plus tard :

Open Banking.

Bank account synchronization.

Receipt scanning.

OCR.

Automatic categorization.

Machine Learning.

AI financial assistant.

Expense anomaly detection.

Subscription detection.

Multi-currency.

Investment tracking.

---

# 78. IA — évolution future

Une couche IA pourra proposer :

catégorisation automatique.

analyse des dépenses.

détection des dépenses inhabituelles.

prévision budgétaire.

conseils d'épargne.

Elle ne doit pas être incluse dans le premier MVP.

---

# 79. MVP

Le premier MVP doit contenir :

Authentication.

Household.

Accounts.

Categories.

Transactions.

Monthly Budget.

Dashboard.

Notifications.

---

# 80. Phase 2

Ajouter :

Recurring Transactions.

Savings Goals.

Forecast.

Analytics.

---

# 81. Phase 3

Ajouter :

Mobile app.

Push notifications.

Real-time updates.

Advanced charts.

---

# 82. Phase 4

Ajouter :

Bank synchronization.

Import.

AI.

Machine Learning.

---

# 83. Stratégie de développement Codex

Ne pas générer toute l'application en une seule fois.

Codex doit travailler par incréments.

Ordre :

1. créer repository ;
2. créer architecture Nx ;
3. créer infrastructure Docker ;
4. créer Identity ;
5. créer Finance Service ;
6. implémenter Accounts ;
7. implémenter Transactions ;
8. RabbitMQ ;
9. Outbox ;
10. Budget Service ;
11. Dashboard ;
12. Angular Web ;
13. tests ;
14. observabilité ;
15. mobile.

Chaque étape doit compiler et être testable avant de passer à la suivante.

---

# 84. Definition of Done

Une fonctionnalité est terminée uniquement lorsque :

le code compile.

les tests passent.

les règles métier sont couvertes.

l'API est documentée.

les erreurs sont gérées.

les logs sont présents.

la validation est implémentée.

la sécurité est vérifiée.

Docker fonctionne.

---

# 85. Standards de code

Backend :

C# moderne.

Nullable enabled.

Async / await.

CancellationToken.

Dependency Injection.

Records pour DTO/events.

No magic strings.

No static service locator.

SOLID.

---

# 86. Frontend standards

TypeScript strict mode.

Standalone Angular Components.

Signals.

Lazy loading.

Route guards.

HTTP interceptors.

Reusable components.

Responsive design.

---

# 87. Documentation

Le repository doit contenir :

README.md.

ARCHITECTURE.md.

CONTRIBUTING.md.

docs/.

Documentation :

architecture.

microservices.

events.

database.

local setup.

deployment.

---

# 88. ADR

Utiliser Architecture Decision Records.

Exemples :

ADR-001 PostgreSQL.

ADR-002 RabbitMQ.

ADR-003 MassTransit.

ADR-004 Angular/Ionic.

ADR-005 Event-driven architecture.

ADR-006 Outbox Pattern.

---

# 89. Diagrammes

Documenter avec Mermaid :

System Context.

Container Diagram.

Microservices.

Event Flow.

Transaction Flow.

Budget Flow.

Deployment Architecture.

---

# 90. Premier objectif concret

Le premier scénario End-to-End fonctionnel doit être :

Utilisateur se connecte.

↓

Crée un foyer.

↓

Crée un compte bancaire.

↓

Ajoute une catégorie.

↓

Ajoute une dépense.

↓

La transaction est enregistrée.

↓

TransactionCreated est publiée.

↓

Budget Service reçoit l'événement.

↓

Le budget est recalculé.

↓

Dashboard est mis à jour.

↓

Angular affiche la nouvelle situation.

Ce scénario constitue la première véritable Vertical Slice complète du système.
