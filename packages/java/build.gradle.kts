import com.vanniktech.maven.publish.SonatypeHost

plugins {
    `java-library`
    id("com.vanniktech.maven.publish") version "0.30.0"
}

group = "io.axene"
version = "0.1.0"

repositories {
    mavenCentral()
}

java {
    withSourcesJar()
    withJavadocJar()
}

tasks.withType<JavaCompile>().configureEach {
    // Library targets Java 11 (needs java.net.http.HttpClient) regardless of the build JDK.
    options.release.set(11)
}

dependencies {
    implementation("com.fasterxml.jackson.core:jackson-databind:2.17.2")
    testImplementation(platform("org.junit:junit-bom:5.10.3"))
    testImplementation("org.junit.jupiter:junit-jupiter")
    testRuntimeOnly("org.junit.platform:junit-platform-launcher")
}

tasks.test {
    useJUnitPlatform()
}

mavenPublishing {
    // New Sonatype Central Portal; auto-release once the upload validates.
    publishToMavenCentral(SonatypeHost.CENTRAL_PORTAL, automaticRelease = true)
    signAllPublications()
    coordinates("io.axene", "mailer", version.toString())

    pom {
        name.set("Axene Mailer")
        description.set("Official Java client for Axene Mailer: send receipts, confirmations, and campaigns from your own domain. Priced in KES, billed via M-Pesa.")
        inceptionYear.set("2026")
        url.set("https://github.com/Axene-Solutions/axene-sdks")
        licenses {
            license {
                name.set("MIT")
                url.set("https://opensource.org/licenses/MIT")
            }
        }
        developers {
            developer {
                id.set("axene")
                name.set("Axene Solutions")
                url.set("https://axene.io")
            }
        }
        scm {
            url.set("https://github.com/Axene-Solutions/axene-sdks")
            connection.set("scm:git:git://github.com/Axene-Solutions/axene-sdks.git")
            developerConnection.set("scm:git:ssh://git@github.com/Axene-Solutions/axene-sdks.git")
        }
    }
}
