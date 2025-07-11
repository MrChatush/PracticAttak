# 🚚 DeliveryOptimizer - Оптимизация маршрутов доставки

## 📌 О проекте
WPF-приложение для оптимизации маршрутов курьерской доставки с возможностью динамического добавления заказов и учетом их приоритетов. Реализует алгоритм ближайшего соседа для решения задачи коммивояжера.

---

## 🚀 Основные возможности
- **Оптимизация маршрутов** с помощью алгоритма ближайшего соседа
- **6 предустановленных сценариев** доставки (центр города, окраины и др.)
- **Динамическое добавление** заказов через клик по карте
- **Гибкая система приоритетов** (0.0-1.0)
- **Визуализация маршрутов** с отображением склада и точек доставки
- **Подробная информация** о маршруте и порядке доставки

---

## 🛠 Технические особенности
### Алгоритмы
- Алгоритм ближайшего соседа для задачи коммивояжера
- Валидация маршрутов с расчетом длины пути

### Технологии
- **WPF**, **C#**, **.NET**
- **MVVM-подход** (частично)

### Архитектура
- **SOLID-принципы**
- Разделение на логические модули:
  - RouteManager - управление маршрутами
  - RouteRenderer - визуализация
  - RouteValidator - проверка маршрутов

---

## 📊 Структура проекта
### Основные классы:
| Класс | Описание |
|-------|----------|
| `RouteManager.cs` | Управление заказами и построение маршрутов |
| `RouteRenderer.cs` | Визуализация маршрута на Canvas |
| `RouteValidator.cs` | Проверка корректности маршрутов |
| `MainWindow.xaml.cs` | Логика главного окна |
| `PriorityInputDialog.cs` | Диалог ввода приоритета |

---

## 📥 Установка и запуск
1. Клонируйте репозиторий:
   ```bash
   git clone https://github.com/MrChatush/PracticAttak.git
2. Откройте решение в Visual Studio:
   Запустите WpfApp3.sln

Учебная информация:

🎓 Группа: ИСП-8

📚 Специальность: 09.02.07 Информационные системы и программирование

📅 Год разработки: 2025
