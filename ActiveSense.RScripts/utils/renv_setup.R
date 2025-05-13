# ==================================
# CHECKS IF RENV::RESTORE IS NEEDED
# ==================================

needs_renv_restore <- function(project_path = ".") {
  if (!requireNamespace("renv", quietly = TRUE)) {
    message("renv package not found. Attempting to install...")
    tryCatch({
      install.packages("renv", repos = "https://cloud.r-project.org/", quiet = TRUE)
      if (!requireNamespace("renv", quietly = TRUE)) {
        stop("Failed to install renv. Please ensure R has permissions or install renv manually.")
      }
    }, error = function(e) {
      stop(paste("Failed to install renv:", conditionMessage(e)))
    })
  }
  
  status_report <- NULL
  capture.output({
    status_report <- suppressMessages(suppressWarnings(renv::status(project = project_path)))
  }, type="message")
  
  
  if (is.null(status_report)) {
    message("Warning: Could not determine renv status. Proceeding with caution.")
    return(TRUE)
  }
  
  if (isFALSE(status_report$synchronized)) {
    message("renv status: Project is not synchronized with renv.lock.")
    return(TRUE)
  }
  
  message("renv status: Project appears synchronized.")
  return(FALSE)
}

# ==================================
# DOES RENV::RESTORE IF NECCESSARY
# ==================================

project_dir <- "."

if (needs_renv_restore(project_path = project_dir)) {
  message("--- R environment requires setup/update. Running renv::restore(). ---")
  message("This may take a few minutes and requires an internet connection if packages are missing.")
  
  Sys.setenv(RENV_CONFIG_CONFIRM = "FALSE")
  
  tryCatch({
    renv::restore(project = project_dir, prompt = FALSE)
    message("--- renv::restore() completed. ---")
  }, error = function(e) {
    message("!!! ERROR during renv::restore(): !!!")
    message(conditionMessage(e))
    stop("Failed to restore R environment. Please check internet connection and logs. Error: ", conditionMessage(e))
  })
  
} else {
  message("--- R environment is up to date. Proceeding with script. ---")
}