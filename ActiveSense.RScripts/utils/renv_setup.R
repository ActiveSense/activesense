# ==================================
# DEACTIVATE SYMLINK
# ==================================

message("Setting renv.config.cache.symlinks to FALSE...")
options(renv.config.cache.symlinks = FALSE)
message("renv.config.cache.symlinks is now: ", getOption("renv.config.cache.symlinks"))


# ==================================
# INSTALL RENV
# ==================================

# Does this happen implicitly?
# tryCatch({
#   install.packages("renv", repos = "https://cloud.r-project.org/", quiet = TRUE)
#   if (!requireNamespace("renv", quietly = TRUE)) {
#     stop("Failed to install renv. Please ensure R has permissions or install renv manually.")
#   }
# }, error = function(e) {
#   stop(paste("Failed to install renv:", conditionMessage(e)))
# })

# ==================================
# RUN RENV::RESTORE
# ==================================

project_dir <- "."

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
  
