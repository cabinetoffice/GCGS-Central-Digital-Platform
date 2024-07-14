import os
import re

# Terraform variables/outputs sorting script

def order_file(directory, file_name, resource_name):
    """For a single specific file, order the resources named for resource_name

    Parameters:
    directory (string): E.g. "/var/www/foo"
    file_name (string): E.g. "variables.tf"
    resource_name (string): The Terraform resource name you want to sort for E.g. "variable"

    Returns:
    int 0 if no problems corrected; 1 if problems corrected
    """

    # we are not interested in ordering files from external modules
    if ".terraform" in directory:
        return 0

    # Open the file
    filepath = os.path.join(directory, file_name)
    f = open(filepath, "r+")
    open_file = f.read()

    # use regex to capture the contents of each resource block - this results in a dictionary
    # of resource names : resource contents, allowing us to sort by resource name,
    # thereby alphabetically sorting the file
    resources = re.findall(resource_name + " (.*{(\n|[ E].*)*})", open_file)
    resources.sort(key=lambda x: x[0])

    # this ordered dictionary is then put back into terraform syntax
    output = ""
    for resource in resources:
        output += "{} {}\n\n".format(resource_name, resource[0])
    output = output.rstrip("\n") + "\n"

    # Detect whether the change affects variable ordering
    # (if we check more naively, this script will strip comments and that shows up as a difference)
    old_resources = matching_lines(resource_name, open_file)
    new_resources = matching_lines(resource_name, output)

    if old_resources != new_resources:
        print(f"Changes made to {filepath}")
        f.seek(0)
        f.write(output)
        f.truncate()
        f.close()
        return 1

    # Everything is fine, return 0
    return 0


def matching_lines(resource_name, multi_line_string):
    """Given a string of the contents of a .tf file, filter it, like a very basic grep

    Parameters:
    resource_name (string): The Terraform resource name you want to sort for E.g. "variable"
    multi_line_string (string): The haystack

    Returns:
    string The lines in multi_line_string matching resource_name
    """

    tmp = ""
    for line in multi_line_string.splitlines():
        if re.search(resource_name + ".*", line):
            tmp += "\n" + line
    return tmp.strip()


def sequence_resources_in_files(file_name, resource_name):
    """Iterate through modules and sequence the resources in them

    Parameters:
    file_name (string): E.g. "variables.tf"
    resource_name (string): The Terraform resource name you want to sort for E.g. "variable"

    Returns:
    int Number of problems found
    """

    problems_found = 0
    root_dir = "./modules"
    for directory, subdirectories, files in os.walk(root_dir):
        for file in files:
            if file == file_name:
                problems_found += order_file(directory, file_name, resource_name)

    return problems_found


def main():
    problems_found = 0
    problems_found += sequence_resources_in_files("variables.tf", "variable")
    problems_found += sequence_resources_in_files("outputs.tf", "output")

    if problems_found > 0:
        print(f"{problems_found} files had problems")
        # Exit with a failure code if any problems were found
        exit(1)


if __name__ == "__main__":
    main()
