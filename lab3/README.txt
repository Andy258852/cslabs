Реализована переносимая система конфигурации. Представлена в виде статического класса OptionManager.
В этом классе есть изменяемые поля:
public static string DefaultXmlPath { get; set; } = "config.xml"; //адрес xml
public static string DefaultXsdPath { get; set; } = "config.xsd"; //адрес xsd
public static string DefaultJsonPath { get; set; } = "config.json"; //адрес json
public static string DefaultJsonSchemaPath { get; set; } = "config.jschema"; //адрес json schema
public static string LogPath { get; set; } = "log.txt"; //адрес лога
public static bool ValidatingWithSchema { get; set; } = true; //Проверить ли xml через xsd и json через json schema
public static bool LogEnabled { get; set; } = true; //Записывать ли события
public static bool XmlIsHigher { get; set; } = true; //Устанавливает приоритет xml файла при равных условиях.

Как происходит выбор между xml и json:
Проверяется на валидность по схеме для того и того, если ValidatingWithSchema == true, если что-то одно провильное, другое нет, то выбирает правильную опцию, если
оба не есть правильные, то выдает исключения, если оба правильные или валидаци через схемы отключена, происходит считывание опций из xml и json, естественно с проверкой на
соответствие синтаксису json и xml, опять же если оба правильные, считвыается кол-во необходимых опций, у кого больше, оттуда берем (при условии, что все обязательные опции
существуют). При этом все логируется при LogEnabled == true.

Для реализации ввода есть структура Option, которая представляет один считываемый параметр со всеми его настройками (Обязателен ли он, значение по умолчанию, тип, как оно названо в
config).

Есть два метода:
получить одну опцию(Вводится одна Option, возврацается значение типа object, которое является заданным типом, но упакованным)
получить все опции(Вводится сколько угодно опций через , возвращается массив значений, в заданном порядке по Option)

В целом система заточена на то, чтобы хватать элементы json и элементы + аттрибуты из xml по установленным псевдонимам, не обращая внимания на установленную иерархию (если отключена валидация через схемы).
Сервис представлен, как бибилиотека(Configurator).
Интегрируется в любые проекта.

Установлено несколь ко пакетов NuGet:
System.Json;
Newtonsoft.Json.Schema;
Newtonsoft.Json;
с дочерними.

