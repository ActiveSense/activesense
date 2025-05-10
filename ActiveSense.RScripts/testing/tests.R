# ==================================
# REGRESSION TESTING
# ==================================

test_that("regression_test_typical", {
  # set working directory
  setwd(dirname(getwd()))
  
  # clean up
  directories <- c(
    file.path(getwd(), "data"),
    file.path(getwd(), "outputs")
  )
  
  for (dir in directories) {
    unlink(list.files(dir, full.names = TRUE, recursive = FALSE), recursive = TRUE)
  }
  
  # copying bin files into /data/ folder
  files_to_copy <- list.files("testing/references/bin", full.names = TRUE)
  file.copy(from = files_to_copy, to = "data")

  # run analysis
  main_script_path <- file.path(getwd(), "_main.R")
  source(main_script_path)
  
  # compare newly generated files with the correct csv folder
  output_dir <- file.path(getwd(), "/outputs")
  correct_csv_dir <- file.path(getwd(), "/testing/references/correct_csv")
  
  # Get list of files in output directory
  output_files <- list.files(output_dir, pattern = "\\.csv$", full.names = FALSE, recursive = TRUE)
  
  # Loop through each output file and compare with reference
  for (file_name in output_files) {
    # Read csv files
    output_data <- read.csv(file.path(output_dir, file_name), stringsAsFactors = FALSE)
    reference_data <- read.csv(file.path(correct_csv_dir, file_name), stringsAsFactors = FALSE)
    
    # Compare content
    comparison <- all.equal(output_data, reference_data)
    
    # Assert equality
    expect_true(isTRUE(comparison),
                info = paste("Files differ:", file_name, "-", comparison))
  }
  
})

#test_that("regression_test_typical_without_mmap", {
#  
#}

#test_that("regression_test_empty", {
#  
#}

#test_that("regression_test_single", {
#  
#}

#test_that("regression_test_many", {
#  
#}

#test_that("test_multiple_files", {
#  
#}