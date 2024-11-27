import sys
import csv
import os
from xml.etree.ElementTree import parse


def load_csv_to_dict(csv_file):
    nested_dict = {}
    with open(csv_file, mode="r", encoding="utf-8") as file:
        reader = csv.DictReader(file)
        for row in reader:
            key = row["Key"]
            if key:
                nested_dict[key] = {"English": row.get("English", ""), "Welsh": row.get("Welsh", "")}
    return nested_dict


def get_original_entries(xml_file):
    tree = parse(xml_file)
    root = tree.getroot()

    entries = []
    for data in root.findall("data"):
        key = data.get("name")
        value_element = data.find("value")
        value = value_element.text if value_element is not None else None
        if key:
            entries.append({"Key": key, "Value": value})
    return entries


def escape(value):
    return (value
            .replace("&", "&amp;")
            .replace("<", "&lt;")
            .replace(">", "&gt;"))


def create_resx(entries, csv_dict, language, template_file, output_file):
    with open(template_file, "r", encoding="utf-8") as template:
        template_content = template.read()

    translations = []

    # Loop over existing entries in the XML file first to update them so they stay in the same order
    for entry in entries:
        key = entry["Key"]
        original_value = entry["Value"]
        csv_value = csv_dict.get(key, {}).get(language)
        value_to_use = csv_value if csv_value else original_value

        escaped_value = escape(value_to_use)
        translations.append(f'  <data name="{key}" xml:space="preserve">\n    <value>{escaped_value}</value>\n  </data>')

    # Now process any entries from the CSV that aren't in the XML file
    original_keys = {entry["Key"] for entry in entries}
    for key, values in csv_dict.items():
        if key not in original_keys:
            csv_value = values.get(language)
            if csv_value:
                escaped_value = escape(csv_value)
                translations.append(f'  <data name="{key}" xml:space="preserve">\n    <value>{escaped_value}</value>\n  </data>')

    resx_content = template_content.replace("<!-- TRANSLATIONS -->", "\n".join(translations))

    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    with open(output_file, "w", encoding="utf-8", newline="\r\n") as output:
        output.write(resx_content)

    print(f".resx file created: {output_file}")


def convert_csv_to_resx(csv_file, template_file, english_resx, welsh_resx):
    csv_dict = load_csv_to_dict(csv_file)

    original_english_entries = get_original_entries(english_resx)
    create_resx(original_english_entries, csv_dict, "English", template_file, english_resx)

    original_welsh_entries = get_original_entries(welsh_resx)
    create_resx(original_welsh_entries, csv_dict, "Welsh", template_file, welsh_resx)



if __name__ == "__main__":
    csv_file = sys.argv[1]
    template_file = sys.argv[2]
    english_resx = sys.argv[3]
    welsh_resx = sys.argv[4]

    convert_csv_to_resx(csv_file, template_file, english_resx, welsh_resx)
