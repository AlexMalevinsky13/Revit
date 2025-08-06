# Revit.FamilyEditor

## О проекте
Данный проект выполнен в рамках тестового задания для ООО "Бергманн Инфотех Групп Ост".

Задача включала:
- Полное воспроизведение заданного семейства Revit с нуля через Revit API
- Привязку размеров к параметру w и параметру Extrusion End
- Создание всей геометрии, параметров и размеров исключительно через API, без использования ElementTransformUtils.CopyElements и других методов копирования
- Сохранение всех данных, необходимых для создания семейства, в сериализованном виде и их последующую десериализацию с восстановлением семейства
- Автоматическое открытие воссозданного семейства.

Проект может быть полезен разработчикам, изучающим Revit API, и как пример автоматизации генерации семейств.

## Установка

1. Соберите проект `Revit.FamilyEditor` и получите `.dll` сборку. В проекте могут присутствовать дополнительные сборки, с сегментом *.Lesson.*. Они используются исключительно для учебных или тестовых целей.
2. Поместите её в любое удобное место на диске (например, в папку `~/RevitPlugins/FamilyEditor/`).
3. Создайте `.addin`-файл или используйте уже готовый (пример ниже).
4. Разместите `.addin`-файл в директории: %AppData%\Autodesk\Revit\Addins\2024
5. В `.addin`-файле укажите абсолютный путь до вашей сборки (`Assembly`). Файл находится в директории Config

## Пример `.addin`-файла

```xml
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<RevitAddIns>
  <AddIn Type="Command">
    <Name>ExportFamily</Name>
    <Assembly>ПУТЬ_ДО_СБОРКИ\Revit.FamilyEditor.dll</Assembly>
    <AddInId>6E0AA8C2-5678-4D3A-9F9B-0DABCE123446</AddInId>
    <FullClassName>Revit.FamilyEditor.ExportFamily</FullClassName>
    <VendorId>RFE</VendorId>
    <VendorDescription>Плагин FamilyEditor</VendorDescription>
  </AddIn>

  <AddIn Type="Command">
    <Name>ImportFamily</Name>
    <Assembly>ПУТЬ_ДО_СБОРКИ\Revit.FamilyEditor.dll</Assembly>
    <AddInId>6E0AA8C2-5678-4D3A-9F9B-0DABCE133446</AddInId>
    <FullClassName>Revit.FamilyEditor.ImportFamily</FullClassName>
    <VendorId>RFE</VendorId>
    <VendorDescription>Плагин FamilyEditor</VendorDescription>
  </AddIn>
</RevitAddIns>
```

## Использование

После запуска Revit и открытия семейства:

- Перейдите в меню **Надстройки → Внешние инструменты**.
- Вы увидите две команды:
- **ExportFamily** — экспортирует данные текущего семейства в JSON.
- **ImportFamily** — создает новое семейство на основе ранее сохраненного JSON.

В каталоге проекта присутствуют JSON-файлы с примерами данных, которые можно использовать для теста или демонстрации работы плагина.

## Зависимости

- Autodesk Revit 2024
- .NET Framework 4.8
- RevitAPI.dll и RevitAPIUI.dll (должны быть доступны в проекте)
