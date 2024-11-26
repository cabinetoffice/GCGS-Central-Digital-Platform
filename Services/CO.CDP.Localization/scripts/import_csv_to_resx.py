import sys
import csv
import os
from xml.etree.ElementTree import Element, SubElement, ElementTree, parse


def get_original_order(xml_file):
    tree = parse(xml_file)
    root = tree.getroot()

    order = []
    for data in root.findall("data"):
        key = data.get("name")
        if key:
            order.append(key)
    return order


def sort_csv_data_by_order(data, order):
    order_index = {key: i for i, key in enumerate(order)}

    return sorted(data, key=lambda row: order_index.get(row["Key"], float("inf")))


def escape(value):
    return (value
            .replace("&", "&amp;")
            .replace("<", "&lt;")
            .replace(">", "&gt;"))


def create_resx(data, language, template_file, output_file):
    with open(template_file, "r", encoding="utf-8") as template:
        template_content = template.read()

    translations = []
    for row in data:
        key = row["Key"]
        value = row[language]
        if not key or not value.strip():
            continue

        escaped_value = escape(value)
        translations.append(f'  <data name="{key}" xml:space="preserve">\n    <value>{escaped_value}</value>\n  </data>')

    resx_content = template_content.replace("<!-- TRANSLATIONS -->", "\n".join(translations))

    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    with open(output_file, "w", encoding="utf-8", newline="\r\n") as output:
        output.write(resx_content)

    print(f".resx file created: {output_file}")


def convert_csv_to_resx(csv_file, template_file, english_resx, welsh_resx):
    original_order = get_original_order(english_resx)

    with open(csv_file, mode="r", encoding="utf-8") as file:
        reader = csv.DictReader(file)
        data = list(reader)

    sorted_data = sort_csv_data_by_order(data, original_order)

    create_resx(sorted_data, "English", template_file, english_resx)
    create_resx(sorted_data, "Welsh", template_file, welsh_resx)


if __name__ == "__main__":
    csv_file = sys.argv[1]
    template_file = sys.argv[2]
    english_resx = sys.argv[3]
    welsh_resx = sys.argv[4]

    convert_csv_to_resx(csv_file, template_file, english_resx, welsh_resx)
