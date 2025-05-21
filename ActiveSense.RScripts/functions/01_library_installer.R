
#' @name 01_library_installer
#' @title library installer
#'
#' @description Installs multiple libraries as once
#'
#' @param librarys A vector of strings of libraries that are required to install.
#'

library_installer <- function(librarys) { # 'librarys' is a vector of package names
  # Get current repository settings once, to show the user
  current_repos <- getOption("repos")
  
  # Print a general message about repositories being used
  if (length(current_repos) == 0 || (length(current_repos) == 1 && current_repos[1] == "@CRAN@")) {
    message("INFO: No specific repositories are configured, or only the default CRAN placeholder is set.")
    message("      'install.packages()' will likely prompt you to choose a CRAN mirror or use a default.")
  } else {
    message("INFO: 'install.packages()' will use the following repositories (in this order):")
    # Nicely print named repositories
    if (!is.null(names(current_repos))) {
      for (repo_name in names(current_repos)) {
        if (nzchar(repo_name)) { # Check if the name is not empty
          message(paste0("      - ", repo_name, ": ", current_repos[repo_name]))
        } else { # Handle cases where some might be unnamed if mixed
          message(paste0("      - (Unnamed): ", current_repos[repo_name]))
        }
      }
    } else { # All repositories are unnamed
      for(repo_url in current_repos){
        message(paste0("      - (Unnamed): ", repo_url))
      }
    }
  }
  message("--- Starting package checks ---")
  
  for (pkg_name in librarys) {
    message(paste0("\nChecking for package: '", pkg_name, "'...")) # Add a newline for readability per package
    if (pkg_name %in% rownames(installed.packages()) == FALSE) {
      message(paste0("   -> '", pkg_name, "' is NOT installed. Attempting installation..."))
      
      # install.packages will use the repositories from getOption("repos")
      # R tries them in the order they are listed in getOption("repos")
      install.packages(pkg_name, dependencies = TRUE)
      
      # After attempting installation, check again
      if (pkg_name %in% rownames(installed.packages())) {
        message(paste0("   SUCCESS: '", pkg_name, "' was installed."))
        message(paste0("      It was installed using your configured repositories (listed above)."))
        message(paste0("      R would have used the *first* repository in that list where '", pkg_name, "' was found."))
      } else {
        message(paste0("   FAILURE: '", pkg_name, "' could NOT be installed. See messages above from install.packages()."))
      }
    } else {
      message(paste0("   -> '", pkg_name, "' is ALREADY installed."))
    }
  }
  message("\n--- Package check complete ---")
}
