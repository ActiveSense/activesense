# ==================================
# DEACTIVATE SYMLINK
# ==================================

tryCatch({
  message("Setting renv.config.cache.symlinks to FALSE...")
  options(renv.config.cache.symlinks = FALSE)
  message("renv.config.cache.symlinks is now: ", getOption("renv.config.cache.symlinks"))
}, error = function(e) {
  message("!!! ERROR during switching config: !!!")
  message(conditionMessage(e))
  stop("Failed to change symlinks. Error: ", conditionMessage(e))
})

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
  
