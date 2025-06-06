
#' @name 01_library_installer
#' @title library installer
#'
#' @description Installs multiple libraries as once
#'
#' @param librarys A vector of strings of libraries that are required to install.
#'

# Surpress checking for newer binaries to source. 
options(install.packages.check.source = "no")

# ==================================
# INSTALLER FUNCTION
# ==================================

install_libraries <- function(libraries) {
  libraries_to_install <- check_if_install_needed(libraries)
  
  if (length(libraries_to_install) == 0) {
    message("###### EVERYTHING'S ALREADY INSTALLED ######")
    return()
  } else {
    message("###### THE FOLLOWING PACKAGES WILL BE INSTALLED FOR --- ", toupper(Sys.info()["sysname"]), " --- ######")
    message(paste(libraries_to_install, collapse = "\n"))
  }
  
  for (pkg_name in libraries_to_install) {
    message(paste0("\n==== Installing '", pkg_name, "' ===="))
    install_package(pkg_name)
    
    success <- pkg_name %in% rownames(installed.packages())
    if (success) {
      message(paste0("\n-> Package ", pkg_name, " successfully  installed.\n"))
    } else {
      message(paste0("\n-> Package ", pkg_name, " couldn't be installed.\n"))
      stop("!!!ERROR: '", pkg_name, "' package couldn't be installed.")
    }
  }
  
}

# ==================================
# HELPER FUNCTIONS
# ==================================

install_package <- function(pkg) {
  os_type <- Sys.info()["sysname"]
  
  if (os_type == "Windows" || os_type == "Darwin") {
    install.packages(pkg, type = "binary", lib = "./libraries")
  } else if (os_type == "Linux") {
    install.packages(pkg, lib = "./libraries")
  }
}

check_if_install_needed <- function(libraries) {
  
  message(paste0("###### CHECK IF INSTALLATION NEEDED ######\n"))
  
  temp_libs = c()
  
  for (pkg_name in libraries) {
    message(paste0("==== Check for ", pkg_name, " ===="))
    
    isInstalled <- pkg_name %in% rownames(installed.packages())
    isGENEApackage <- grepl("GENEA", pkg_name)
    
    # Already installed, but wrong version
    if (isInstalled && isGENEApackage) {
      repo_info <- packageDescription(pkg_name, fields = "Repository")
      
      expected_pattern <- if (use_legacy) "CRAN" else "activesense"
      version_type <- if (use_legacy) "LEGACY (CRAN)" else "CUSTOM"
      
      is_correct_version <- !is.na(repo_info) && grepl(expected_pattern, repo_info)
      
      if (is_correct_version) {
        message(sprintf(" -> Correct %s version installed. No installation necessary\n", version_type))
      } else {
        message(sprintf(" -> installed, but not %s version. Reinstallation necessary.\n", version_type))
        temp_libs <- c(temp_libs, pkg_name)
      }
      
      next
    }
    
    # Not installed
    if (!isInstalled) {
      message(paste0("-> MISSING. Installation neccessary.\n"))
      temp_libs <- c(temp_libs, pkg_name)
      
      next
    }
    
    # Already installed
    if (isInstalled) {
      message(paste0("-> ALREADY installed. No Installation neccessary.\n"))
    }
  }
  
  return(temp_libs)
}