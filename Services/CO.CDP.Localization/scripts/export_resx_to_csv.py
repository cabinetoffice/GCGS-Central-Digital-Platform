import xml.etree.ElementTree as ET
import csv
import sys
import os

def parse_resx(file_path):
    translations = {}
    tree = ET.parse(file_path)
    root = tree.getroot()
    for data in root.findall("data"):
        key = data.get("name")
        value = data.find("value").text if data.find("value") is not None else ""
        if key:
            translations[key] = value

    return translations

def merge_resx_to_csv(english_resx, welsh_resx, output_csv):
    english_translations = parse_resx(english_resx)
    welsh_translations = parse_resx(welsh_resx)

    all_keys = sorted(set(english_translations.keys()).union(welsh_translations.keys()))

    os.makedirs(os.path.dirname(output_csv), exist_ok=True)
    with open(output_csv, mode="w", encoding="utf-8", newline="") as csv_file:
        writer = csv.writer(csv_file)
        writer.writerow(["Key", "English", "Welsh"])

        for key in all_keys:
            english_value = english_translations.get(key, "")
            welsh_value = welsh_translations.get(key, "")
            writer.writerow([key, english_value, welsh_value])

    print(f"CSV file created: {output_csv}")

if __name__ == "__main__":
    english_resx_path = sys.argv[1]
    welsh_resx_path = sys.argv[2]
    output_csv_path = sys.argv[3]

    merge_resx_to_csv(english_resx_path, welsh_resx_path, output_csv_path)
